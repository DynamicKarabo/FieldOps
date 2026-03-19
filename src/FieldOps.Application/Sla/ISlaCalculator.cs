using FieldOps.Domain.Entities;
using FieldOps.Domain.ValueObjects;

namespace FieldOps.Application.Sla;

public interface ISlaCalculator
{
    SlaDeadlines CalculateDeadlines(DateTimeOffset createdAt, SlaConfig config);
    DateTimeOffset CalculateEscalationTime(DateTimeOffset createdAt, SlaConfig config);
}
