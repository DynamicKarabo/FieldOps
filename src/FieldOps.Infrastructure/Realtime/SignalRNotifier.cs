using FieldOps.Application.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace FieldOps.Infrastructure.Realtime;

public class SignalRNotifier : IJobNotificationService
{
    private readonly IHubContext<JobHub> _hubContext;

    public SignalRNotifier(IHubContext<JobHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyStatusChanged(string groupName, Guid jobId, string newStatus)
    {
        await _hubContext.Clients.Group(groupName)
            .SendAsync("JobStatusChanged", new { JobId = jobId, Status = newStatus });
    }

    public async Task NotifyEscalationTriggered(string groupName, Guid jobId, string reason)
    {
        await _hubContext.Clients.Group(groupName)
            .SendAsync("JobEscalated", new { JobId = jobId, Reason = reason });
    }

    public async Task NotifySlaBreached(string groupName, Guid jobId, string breachType)
    {
        await _hubContext.Clients.Group(groupName)
            .SendAsync("JobSlaBreached", new { JobId = jobId, BreachType = breachType });
    }
}

public class JobHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var groupName = Context.GetHttpContext()?.Request.Query["group"].ToString();
        if (!string.IsNullOrWhiteSpace(groupName))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        await base.OnConnectedAsync();
    }

    public Task JoinGroup(string groupName)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public Task LeaveGroup(string groupName)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
