using FieldOps.Application.Common.Exceptions;
using FieldOps.Application.Common.Interfaces;
using FieldOps.Application.Realtime;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;
using FieldOps.Domain.StateMachines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Application.Jobs.Commands;

public record OnSiteJobCommand(Guid JobId, string TriggeredBy, Guid? CorrelationId = null) : IRequest;

public class OnSiteJobCommandHandler : IRequestHandler<OnSiteJobCommand>
{
    private readonly IFieldOpsDbContext _context;
    private readonly IJobNotificationService _notificationService;

    public OnSiteJobCommandHandler(IFieldOpsDbContext context, IJobNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(OnSiteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .Include(j => j.AssignedTechnician)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job == null) throw new NotFoundException(nameof(Job), request.JobId);

        JobStateMachine.EnsureValidTransition(job.Status, JobStatus.OnSite);

        var previousStatus = job.Status;
        var now = DateTimeOffset.UtcNow;
        job.Status = JobStatus.OnSite;
        job.OnSiteAt = now;
        job.UpdatedAt = now;

        if (job.AssignedTechnician != null)
        {
            TechnicianStateMachine.EnsureValidTransition(job.AssignedTechnician.Status, TechnicianStatus.OnSite);
            job.AssignedTechnician.Status = TechnicianStatus.OnSite;
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
            CorrelationId = request.CorrelationId ?? Guid.NewGuid()
        };

        _context.JobStatusEvents.Add(statusEvent);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyStatusChanged(GetJobGroupName(job.Id), job.Id, job.Status.ToString());
    }

    private static string GetJobGroupName(Guid jobId) => $"job-{jobId}";
}
