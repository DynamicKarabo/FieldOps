using FieldOps.Api.Controllers;
using FieldOps.Application.Jobs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FieldOps.ScenarioTests;

public class JobsControllerScenarioTests
{
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenJobIsMissing()
    {
        var mediator = new NullJobMediator();
        var controller = new JobsController(mediator);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private sealed class NullJobMediator : IMediator
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetJobByIdQuery)
            {
                return Task.FromResult<TResponse>(default!);
            }

            throw new NotSupportedException("This test mediator only supports GetJobByIdQuery.");
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            throw new NotSupportedException("This test mediator only supports GetJobByIdQuery.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("This test mediator only supports GetJobByIdQuery.");
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("This test mediator only supports GetJobByIdQuery.");
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("This test mediator only supports GetJobByIdQuery.");
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("This test mediator only supports GetJobByIdQuery.");
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            throw new NotSupportedException("This test mediator only supports GetJobByIdQuery.");
        }
    }
}
