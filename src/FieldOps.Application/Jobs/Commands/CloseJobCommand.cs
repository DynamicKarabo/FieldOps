using FieldOps.Application.Common.Exceptions;
using FieldOps.Application.Common.Interfaces;
using FieldOps.Application.Realtime;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;
using FieldOps.Domain.StateMachines;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FieldOps.Application.Jobs.Commands;

public record CloseJobCommand(Guid JobId, string ResolutionNotes, string TriggeredBy) : IRequest;

public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand>
{
    private readonly IFieldOpsDbContext _context;
    private readonly IJobNotificationService _notificationService;
    private readonly ILogger<CloseJobCommandHandler> _logger;

    public CloseJobCommandHandler(
        IFieldOpsDbContext context,
        IJobNotificationService notificationService,
        ILogger<CloseJobCommandHandler> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .Include(j => j.AssignedTechnician)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job == null) throw new NotFoundException(nameof(Job), request.JobId);

        JobStateMachine.EnsureValidTransition(job.Status, JobStatus.Closed);

        var previousStatus = job.Status;
        job.Status = JobStatus.Closed;
        job.ClosedAt = DateTimeOffset.UtcNow;
        job.Notes = request.ResolutionNotes;
        job.UpdatedAt = DateTimeOffset.UtcNow;

        if (job.AssignedTechnician != null)
        {
            TechnicianStateMachine.EnsureValidTransition(job.AssignedTechnician.Status, TechnicianStatus.Available);
            job.AssignedTechnician.Status = TechnicianStatus.Available;
            job.AssignedTechnician.CurrentJobId = null;
            job.AssignedTechnician.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var statusEvent = new JobStatusEvent
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            ClientId = job.ClientId,
            PreviousStatus = previousStatus,
            NewStatus = job.Status,
            TriggeredBy = request.TriggeredBy,
            TriggerSource = JobTriggerSource.Api,
            OccurredAt = DateTimeOffset.UtcNow,
            ReceivedAt = DateTimeOffset.UtcNow,
            CorrelationId = Guid.NewGuid(),
            Notes = request.ResolutionNotes
        };

        _context.JobStatusEvents.Add(statusEvent);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _notificationService.NotifyStatusChanged(GetJobGroupName(job.Id), job.Id, job.Status.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify status change for job {JobId} with status {JobStatus}", job.Id, job.Status);
            // TODO: Replace direct notifications with an outbox pattern for guaranteed delivery.
        }
    }

    private static string GetJobGroupName(Guid jobId) => $"job-{jobId}";
}
