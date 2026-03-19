using FieldOps.Domain.Enums;
using FieldOps.Domain.StateMachines;

namespace FieldOps.UnitTests.Domain;

public class JobStateMachineTests
{
    [Theory]
    [InlineData(JobStatus.Created, JobStatus.Assigned, true)]
    [InlineData(JobStatus.Created, JobStatus.Cancelled, true)]
    [InlineData(JobStatus.Assigned, JobStatus.Acknowledged, true)]
    [InlineData(JobStatus.Acknowledged, JobStatus.EnRoute, true)]
    [InlineData(JobStatus.EnRoute, JobStatus.OnSite, true)]
    [InlineData(JobStatus.OnSite, JobStatus.Closed, true)]
    [InlineData(JobStatus.OnSite, JobStatus.Paused, true)]
    [InlineData(JobStatus.OnSite, JobStatus.Escalated, true)]
    [InlineData(JobStatus.Paused, JobStatus.OnSite, true)]
    [InlineData(JobStatus.Escalated, JobStatus.Assigned, true)]
    [InlineData(JobStatus.Created, JobStatus.Closed, false)]
    [InlineData(JobStatus.Closed, JobStatus.Assigned, false)]
    public void CanTransition_ReturnsExpectedResult(JobStatus current, JobStatus next, bool expected)
    {
        var result = JobStateMachine.CanTransition(current, next);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnsureValidTransition_ThrowsOnInvalid()
    {
        Assert.Throws<InvalidOperationException>(() => 
            JobStateMachine.EnsureValidTransition(JobStatus.Created, JobStatus.Closed));
    }
}
