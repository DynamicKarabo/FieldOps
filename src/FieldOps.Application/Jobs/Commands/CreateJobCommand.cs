using FieldOps.Application.Common.Interfaces;
using FieldOps.Application.Sla;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;
using FieldOps.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Application.Jobs.Commands;

public record CreateJobCommand(
    Guid ClientId,
    string JobType,
    JobPriority Priority,
    string Title,
    string Description,
    Address SiteAddress,
    string ContactName,
    string ContactPhone,
    string[] RequiredSkills,
    decimal? SiteLatitude = null,
    decimal? SiteLongitude = null) : IRequest<Guid>;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, Guid>
{
    private readonly IFieldOpsDbContext _context;
    private readonly ISlaCalculator _slaCalculator;

    public CreateJobCommandHandler(IFieldOpsDbContext context, ISlaCalculator slaCalculator)
    {
        _context = context;
        _slaCalculator = slaCalculator;
    }

    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var slaConfig = await _context.SlaConfigs
            .FirstOrDefaultAsync(s => s.ClientId == request.ClientId && s.JobType == request.JobType && s.Priority == request.Priority, cancellationToken);

        if (slaConfig == null)
        {
            // Fallback to client default or throw
            throw new InvalidOperationException($"No SLA configuration found for JobType {request.JobType} and Priority {request.Priority}");
        }

        var createdAt = DateTimeOffset.UtcNow;
        var deadlines = _slaCalculator.CalculateDeadlines(createdAt, slaConfig);

        var jobId = Guid.NewGuid();
        var referenceSuffix = jobId.ToString("N")[..8].ToUpperInvariant();

        var job = new Job
        {
            Id = jobId,
            ClientId = request.ClientId,
            Reference = $"JOB-{createdAt:yyyyMMdd}-{referenceSuffix}",
            JobType = request.JobType,
            Priority = request.Priority,
            Title = request.Title,
            Description = request.Description,
            SiteAddress = request.SiteAddress,
            SiteLatitude = request.SiteLatitude,
            SiteLongitude = request.SiteLongitude,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            Status = JobStatus.Created,
            SlaConfigId = slaConfig.Id,
            SlaDeadlines = deadlines,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            RequiredSkillsJson = System.Text.Json.JsonSerializer.Serialize(request.RequiredSkills)
        };

        _context.Jobs.Add(job);
        
        var statusEvent = new JobStatusEvent
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            ClientId = job.ClientId,
            NewStatus = JobStatus.Created,
            TriggeredBy = "System",
            TriggerSource = JobTriggerSource.Api,
            OccurredAt = createdAt,
            ReceivedAt = createdAt,
            CorrelationId = Guid.NewGuid()
        };
        
        _context.JobStatusEvents.Add(statusEvent);

        await _context.SaveChangesAsync(cancellationToken);

        return job.Id;
    }
}
