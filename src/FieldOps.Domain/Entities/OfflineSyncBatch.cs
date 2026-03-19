using FieldOps.Domain.Enums;

namespace FieldOps.Domain.Entities;

public class OfflineSyncBatch
{
    public Guid Id { get; set; }
    public Guid TechnicianId { get; set; }
    public Guid ClientId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; set; }
    public int EventCount { get; set; }
    public OfflineSyncStatus Status { get; set; }
    public int FailedEventCount { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
    public Technician Technician { get; set; } = null!;
}
