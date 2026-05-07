using MediatR;

namespace Querify.QnA.Worker.Test.IntegrationTests.Helpers;

public sealed class CapturingMediator : IMediator
{
    public object? LastRequest { get; private set; }
    public int SendCallCount { get; private set; }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        SendCallCount++;
        LastRequest = request;
        return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        SendCallCount++;
        LastRequest = request;
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
        => throw new NotSupportedException();
}
