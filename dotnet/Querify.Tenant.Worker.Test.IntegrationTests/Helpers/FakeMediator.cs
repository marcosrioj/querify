using MediatR;

namespace Querify.Tenant.Worker.Test.IntegrationTests.Helpers;

/// <summary>
/// Minimal IMediator stub for processor tests.
/// Use <see cref="EnqueueException"/> to make the next Send call throw.
/// </summary>
public sealed class FakeMediator : IMediator
{
    private readonly Queue<Exception> _exceptions = new();

    public int SendCallCount { get; private set; }

    public void EnqueueException(Exception ex) => _exceptions.Enqueue(ex);

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        SendCallCount++;
        if (_exceptions.TryDequeue(out var ex))
            throw ex;
        return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        SendCallCount++;
        if (_exceptions.TryDequeue(out var ex))
            throw ex;
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
