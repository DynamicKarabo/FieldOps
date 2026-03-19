using FieldOps.Infrastructure.Sla;
using FieldOps.Domain.Entities;
using FieldOps.Domain.Enums;

namespace FieldOps.UnitTests.Infrastructure;

public class SlaCalculatorTests
{
    private readonly SlaCalculator _calculator = new();

    [Fact]
    public void CalculateDeadlines_ShouldReturnCorrectDeadlines()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var config = new SlaConfig
        {
            ResponseTimeMinutes = 60,
            ResolutionTimeMinutes = 240
        };

        var deadlines = _calculator.CalculateDeadlines(createdAt, config);

        Assert.Equal(createdAt.AddMinutes(60), deadlines.ResponseDeadline);
        Assert.Equal(createdAt.AddMinutes(240), deadlines.ResolutionDeadline);
    }

    [Fact]
    public void CalculateEscalationTime_ShouldReturnCorrectTime()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var config = new SlaConfig
        {
            ResolutionTimeMinutes = 240,
            EscalationThresholdPercent = 75
        };

        var escalationTime = _calculator.CalculateEscalationTime(createdAt, config);

        // 240 * 0.75 = 180
        Assert.Equal(createdAt.AddMinutes(180), escalationTime);
    }
}
