using FieldOps.Application.Common.Exceptions;
using FieldOps.Application.Common.Interfaces;
using FieldOps.Application.Realtime;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;
using FieldOps.Domain.StateMachines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Application.Jobs.Commands;

public record AssignJobCommand(Guid JobId, Guid TechnicianId, string TriggeredBy) : IRequest;

public class AssignJobCommandHandler : IRequestHandler<AssignJobCommand>
{
    private readonly IFieldOpsDbContext _context;
    private readonly IJobNotificationService _notificationService;

    public AssignJobCommandHandler(IFieldOpsDbContext context, IJobNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(AssignJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .Include(j => j.StatusEvents)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job == null) throw new NotFoundException(nameof(Job), request.JobId);

        var technician = await _context.Technicians
            .FirstOrDefaultAsync(t => t.Id == request.TechnicianId, cancellationToken);

        if (technician == null) throw new NotFoundException(nameof(Technician), request.TechnicianId);
        if (technician.CurrentJobId != null)
        {
            throw new InvalidOperationException($"Technician {technician.Id} is already assigned to job {technician.CurrentJobId}.");
        }

        // State Machine Check
        JobStateMachine.EnsureValidTransition(job.Status, JobStatus.Assigned);
        TechnicianStateMachine.EnsureValidTransition(technician.Status, TechnicianStatus.Assigned);

        var previousStatus = job.Status;
        var now = DateTimeOffset.UtcNow;

        // Update Job
        job.Status = JobStatus.Assigned;
        job.AssignedTechnicianId = request.TechnicianId;
        job.UpdatedAt = now;

        // Update Technician
        technician.Status = TechnicianStatus.Assigned;
        technician.CurrentJobId = job.Id;
        technician.UpdatedAt = now;

        // Log Event
        var statusEvent = new JobStatusEvent
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            ClientId = job.ClientId,
            PreviousStatus = previousStatus,
            NewStatus = JobStatus.Assigned,
            TriggeredBy = request.TriggeredBy,
            TriggerSource = JobTriggerSource.Api,
            OccurredAt = now,
            ReceivedAt = now,
            CorrelationId = Guid.NewGuid()
        };
        
        // Correcting PreviousStatus capture:
        // statusEvent.PreviousStatus = previousStatus; 

        _context.JobStatusEvents.Add(statusEvent);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyStatusChanged(GetJobGroupName(job.Id), job.Id, job.Status.ToString());
    }

    private static string GetJobGroupName(Guid jobId) => $"job-{jobId}";
}
