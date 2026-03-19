using FieldOps.Application.Common.Exceptions;
using FieldOps.Application.Common.Interfaces;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;
using FieldOps.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Application.Jobs.Queries;

public record GetJobByIdQuery(Guid Id) : IRequest<JobDto>;

public record JobDto(
    Guid Id,
    string Reference,
    string Title,
    string Description,
    JobStatus Status,
    string JobType,
    JobPriority Priority,
    Address SiteAddress,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AcknowledgedAt,
    DateTimeOffset? EnRouteAt,
    DateTimeOffset? OnSiteAt,
    DateTimeOffset? ClosedAt,
    Guid? AssignedTechnicianId);

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobDto>
{
    private readonly IFieldOpsDbContext _context;

    public GetJobByIdQueryHandler(IFieldOpsDbContext context)
    {
        _context = context;
    }

    public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (job == null) throw new NotFoundException(nameof(Job), request.Id);

        return new JobDto(
            job.Id,
            job.Reference,
            job.Title,
            job.Description,
            job.Status,
            job.JobType,
            job.Priority,
            job.SiteAddress,
            job.CreatedAt,
            job.AcknowledgedAt,
            job.EnRouteAt,
            job.OnSiteAt,
            job.ClosedAt,
            job.AssignedTechnicianId);
    }
}
