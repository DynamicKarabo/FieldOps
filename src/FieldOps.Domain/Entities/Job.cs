using FieldOps.Domain.Enums;
using FieldOps.Domain.ValueObjects;

namespace FieldOps.Domain.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public JobPriority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Address SiteAddress { get; set; } = null!;
    public decimal? SiteLatitude { get; set; }
    public decimal? SiteLongitude { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public Guid? AssignedTechnicianId { get; set; }
    public Guid SlaConfigId { get; set; }
    public SlaDeadlines SlaDeadlines { get; set; } = null!;
    public DateTimeOffset? EscalationSentAt { get; set; }
    public string RequiredSkillsJson { get; set; } = "[]";
    public string? Notes { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public DateTimeOffset? EnRouteAt { get; set; }
    public DateTimeOffset? OnSiteAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
    public Technician? AssignedTechnician { get; set; }
    public SlaConfig SlaConfig { get; set; } = null!;
    public ICollection<JobNote> JobNotes { get; set; } = new List<JobNote>();
    public ICollection<JobStatusEvent> StatusEvents { get; set; } = new List<JobStatusEvent>();
    public ICollection<SlaBreachRecord> BreachRecords { get; set; } = new List<SlaBreachRecord>();
}
