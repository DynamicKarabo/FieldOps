using FieldOps.Application.Sla;
using FieldOps.Domain.Entities;
using FieldOps.Domain.ValueObjects;

namespace FieldOps.Infrastructure.Sla;

public class SlaCalculator : ISlaCalculator
{
    public SlaDeadlines CalculateDeadlines(DateTimeOffset createdAt, SlaConfig config)
    {
        return new SlaDeadlines(
            ResponseDeadline: createdAt.AddMinutes(config.ResponseTimeMinutes),
            ResolutionDeadline: createdAt.AddMinutes(config.ResolutionTimeMinutes)
        );
    }

    public DateTimeOffset CalculateEscalationTime(DateTimeOffset createdAt, SlaConfig config)
    {
        var minutesToEscalation = (config.ResolutionTimeMinutes * config.EscalationThresholdPercent) / 100.0;
        return createdAt.AddMinutes(minutesToEscalation);
    }
}
