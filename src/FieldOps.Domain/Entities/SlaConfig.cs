using FieldOps.Domain.Enums;

namespace FieldOps.Domain.Entities;

public class SlaConfig
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public JobPriority Priority { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public int EscalationThresholdPercent { get; set; }
    public decimal? PenaltyPerBreachZAR { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
}
