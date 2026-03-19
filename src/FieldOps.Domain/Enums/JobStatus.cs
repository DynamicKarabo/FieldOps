namespace FieldOps.Domain.Enums;

public enum JobStatus
{
    Created = 0,
    Assigned = 1,
    Acknowledged = 2,
    EnRoute = 3,
    OnSite = 4,
    Paused = 5,
    Escalated = 6,
    Closed = 7,
    Cancelled = 8
}
