using FieldOps.Domain.Enums;

namespace FieldOps.Domain.StateMachines;

public static class JobStateMachine
{
    private static readonly Dictionary<JobStatus, HashSet<JobStatus>> ValidTransitions = new()
    {
        { JobStatus.Created, new HashSet<JobStatus> { JobStatus.Assigned, JobStatus.Cancelled } },
        { JobStatus.Assigned, new HashSet<JobStatus> { JobStatus.Acknowledged, JobStatus.Cancelled } },
        { JobStatus.Acknowledged, new HashSet<JobStatus> { JobStatus.EnRoute, JobStatus.Cancelled } },
        { JobStatus.EnRoute, new HashSet<JobStatus> { JobStatus.OnSite, JobStatus.Cancelled } },
        { JobStatus.OnSite, new HashSet<JobStatus> { JobStatus.Closed, JobStatus.Paused, JobStatus.Escalated, JobStatus.Cancelled } },
        { JobStatus.Paused, new HashSet<JobStatus> { JobStatus.OnSite, JobStatus.Cancelled } },
        { JobStatus.Escalated, new HashSet<JobStatus> { JobStatus.Assigned, JobStatus.Cancelled } }
    };

    public static bool CanTransition(JobStatus current, JobStatus next)
    {
        if (current == next) return true;
        if (next == JobStatus.Cancelled && !IsTerminal(current)) return true;
        
        return ValidTransitions.TryGetValue(current, out var nextStates) && nextStates.Contains(next);
    }

    public static bool IsTerminal(JobStatus status)
    {
        return status == JobStatus.Closed || status == JobStatus.Cancelled;
    }

    public static void EnsureValidTransition(JobStatus current, JobStatus next)
    {
        if (!CanTransition(current, next))
        {
            throw new InvalidOperationException($"Invalid job status transition from {current} to {next}");
        }
    }
}
