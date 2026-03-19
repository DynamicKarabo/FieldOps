using FieldOps.Domain.Enums;

namespace FieldOps.Domain.Entities;

public class JobStatusEvent
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid ClientId { get; set; }
    public JobStatus? PreviousStatus { get; set; }
    public JobStatus NewStatus { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
    public JobTriggerSource TriggerSource { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Notes { get; set; }
    public Guid CorrelationId { get; set; }

    // Navigation
    public Job Job { get; set; } = null!;
    public Client Client { get; set; } = null!;
}
