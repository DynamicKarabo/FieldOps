using FieldOps.Application.Common.Exceptions;
using FieldOps.Application.Common.Interfaces;
using FieldOps.Application.Realtime;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;
using FieldOps.Domain.StateMachines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Application.Jobs.Commands;

public record EnRouteJobCommand(Guid JobId, string TriggeredBy) : IRequest;

public class EnRouteJobCommandHandler : IRequestHandler<EnRouteJobCommand>
{
    private readonly IFieldOpsDbContext _context;
    private readonly IJobNotificationService _notificationService;

    public EnRouteJobCommandHandler(IFieldOpsDbContext context, IJobNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(EnRouteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .Include(j => j.AssignedTechnician)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job == null) throw new NotFoundException(nameof(Job), request.JobId);

        JobStateMachine.EnsureValidTransition(job.Status, JobStatus.EnRoute);

        var previousStatus = job.Status;
        job.Status = JobStatus.EnRoute;
        job.EnRouteAt = DateTimeOffset.UtcNow;
        job.UpdatedAt = DateTimeOffset.UtcNow;

        if (job.AssignedTechnician != null)
        {
            TechnicianStateMachine.EnsureValidTransition(job.AssignedTechnician.Status, TechnicianStatus.EnRoute);
            job.AssignedTechnician.Status = TechnicianStatus.EnRoute;
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
            CorrelationId = Guid.NewGuid()
        };

        _context.JobStatusEvents.Add(statusEvent);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyStatusChanged(GetJobGroupName(job.Id), job.Id, job.Status.ToString());
    }

    private static string GetJobGroupName(Guid jobId) => $"job-{jobId}";
}
