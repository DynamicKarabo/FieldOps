namespace FieldOps.Application.Realtime;

public interface IJobNotificationService
{
    Task NotifyStatusChanged(string groupName, Guid jobId, string newStatus);
    Task NotifyEscalationTriggered(string groupName, Guid jobId, string reason);
    Task NotifySlaBreached(string groupName, Guid jobId, string breachType);
}
