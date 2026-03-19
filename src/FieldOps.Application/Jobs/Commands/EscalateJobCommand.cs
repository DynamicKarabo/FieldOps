using FieldOps.Application.Common.Exceptions;
using FieldOps.Application.Common.Interfaces;
using FieldOps.Application.Realtime;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;
using FieldOps.Domain.StateMachines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Application.Jobs.Commands;

public record EscalateJobCommand(Guid JobId, string Reason, string TriggeredBy) : IRequest;

public class EscalateJobCommandHandler : IRequestHandler<EscalateJobCommand>
{
    private readonly IFieldOpsDbContext _context;
    private readonly IJobNotificationService _notificationService;

    public EscalateJobCommandHandler(IFieldOpsDbContext context, IJobNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(EscalateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .Include(j => j.AssignedTechnician)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job == null) throw new NotFoundException(nameof(Job), request.JobId);

        JobStateMachine.EnsureValidTransition(job.Status, JobStatus.Escalated);

        var previousStatus = job.Status;
        var now = DateTimeOffset.UtcNow;
        var escalationNote = $"[{now:O}] {request.Reason}";
        job.Status = JobStatus.Escalated;
        job.EscalationSentAt = now;
        job.Notes = string.IsNullOrWhiteSpace(job.Notes)
            ? escalationNote
            : $"{job.Notes}{Environment.NewLine}{escalationNote}";
        job.UpdatedAt = now;

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
            OccurredAt = now,
            ReceivedAt = now,
            CorrelationId = Guid.NewGuid(),
            Notes = request.Reason
        };

        _context.JobStatusEvents.Add(statusEvent);
        await _context.SaveChangesAsync(cancellationToken);

        var groupName = GetJobGroupName(job.Id);
        await _notificationService.NotifyStatusChanged(groupName, job.Id, job.Status.ToString());
        await _notificationService.NotifyEscalationTriggered(groupName, job.Id, request.Reason);
    }

    private static string GetJobGroupName(Guid jobId) => $"job-{jobId}";
}
