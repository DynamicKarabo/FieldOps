using FieldOps.Domain.Enums;

namespace FieldOps.Domain.StateMachines;

public static class TechnicianStateMachine
{
    private static readonly Dictionary<TechnicianStatus, HashSet<TechnicianStatus>> ValidTransitions = new()
    {
        { TechnicianStatus.Offline, new HashSet<TechnicianStatus> { TechnicianStatus.Available } },
        { TechnicianStatus.Available, new HashSet<TechnicianStatus> { TechnicianStatus.Assigned, TechnicianStatus.EnRoute, TechnicianStatus.OnBreak, TechnicianStatus.Offline } },
        { TechnicianStatus.Assigned, new HashSet<TechnicianStatus> { TechnicianStatus.EnRoute, TechnicianStatus.Available, TechnicianStatus.Offline } },
        { TechnicianStatus.EnRoute, new HashSet<TechnicianStatus> { TechnicianStatus.OnSite, TechnicianStatus.Offline } },
        { TechnicianStatus.OnSite, new HashSet<TechnicianStatus> { TechnicianStatus.Available, TechnicianStatus.Offline } },
        { TechnicianStatus.OnBreak, new HashSet<TechnicianStatus> { TechnicianStatus.Available, TechnicianStatus.Offline } }
    };

    public static bool CanTransition(TechnicianStatus current, TechnicianStatus next)
    {
        if (current == next) return true;
        return ValidTransitions.TryGetValue(current, out var nextStates) && nextStates.Contains(next);
    }

    public static void EnsureValidTransition(TechnicianStatus current, TechnicianStatus next)
    {
        if (!CanTransition(current, next))
        {
            throw new InvalidOperationException($"Invalid technician status transition from {current} to {next}");
        }
    }
}
