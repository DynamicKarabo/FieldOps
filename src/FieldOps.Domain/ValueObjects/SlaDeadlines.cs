namespace FieldOps.Domain.ValueObjects;

public record SlaDeadlines(
    DateTimeOffset ResponseDeadline,
    DateTimeOffset ResolutionDeadline,
    bool? ResponseMet = null,
    bool? ResolutionMet = null);
