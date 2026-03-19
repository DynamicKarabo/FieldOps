using FieldOps.Domain.Enums;

namespace FieldOps.Domain.Entities;

public class SlaBreachRecord
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid ClientId { get; set; }
    public BreachType BreachType { get; set; }
    public DateTimeOffset Deadline { get; set; }
    public DateTimeOffset? ActualTime { get; set; }
    public int OverdueMinutes { get; set; }
    public Guid? TechnicianId { get; set; }
    public decimal? PenaltyAmountZAR { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Job Job { get; set; } = null!;
    public Client Client { get; set; } = null!;
}
