using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Querify.QnA.Portal.Business.Source.Queries.InspectExternalUrl;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Source;

public sealed class SourceExternalUrlInspectionCoordinatorTests
{
    private static readonly Uri ExternalUrl = new("https://93.184.216.34/manual.pdf");

    [Fact]
    public async Task InspectAsync_CachesSuccessfulInspection()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var handler = new CountingHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent("pdf"u8.ToArray())
                {
                    Headers =
                    {
                        ContentType = new("application/pdf"),
                        ContentLength = 3
                    }
                }
            }));
        var coordinator = CreateCoordinator(handler, cache);

        var first = await coordinator.InspectAsync(ExternalUrl, CancellationToken.None);
        var second = await coordinator.InspectAsync(ExternalUrl, CancellationToken.None);

        Assert.True(first.IsReachable);
        Assert.True(second.IsReachable);
        Assert.Equal("application/pdf", second.ContentType);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task InspectAsync_DeduplicatesConcurrentInspectionForSameUrl()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var releaseResponse = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new CountingHttpMessageHandler(async _ =>
        {
            await releaseResponse.Task;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent("pdf"u8.ToArray())
            };
        });
        var coordinator = CreateCoordinator(handler, cache);

        var firstTask = coordinator.InspectAsync(ExternalUrl, CancellationToken.None);
        var secondTask = coordinator.InspectAsync(ExternalUrl, CancellationToken.None);

        await WaitUntilAsync(() => handler.RequestCount == 1);
        releaseResponse.SetResult();
        var results = await Task.WhenAll(firstTask, secondTask);

        Assert.All(results, result => Assert.True(result.IsReachable));
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task InspectAsync_RejectsPrivateResolvedAddress()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var handler = new CountingHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var coordinator = CreateCoordinator(handler, cache);
        var privateUrl = new Uri("https://10.0.0.10/manual.pdf");

        var exception = await Assert.ThrowsAsync<Querify.Common.Infrastructure.ApiErrorHandling.Exception.ApiErrorException>(
            () => coordinator.InspectAsync(privateUrl, CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task InspectAsync_RejectsMappedPrivateResolvedAddress()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var handler = new CountingHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var coordinator = CreateCoordinator(handler, cache);
        var privateUrl = new Uri("https://[::ffff:192.168.1.10]/manual.pdf");

        var exception = await Assert.ThrowsAsync<Querify.Common.Infrastructure.ApiErrorHandling.Exception.ApiErrorException>(
            () => coordinator.InspectAsync(privateUrl, CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
        Assert.Equal(0, handler.RequestCount);
    }

    private static SourceExternalUrlInspectionCoordinator CreateCoordinator(
        CountingHttpMessageHandler handler,
        IMemoryCache cache)
    {
        return new(
            new StaticHttpClientFactory(handler),
            cache);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        while (!condition())
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
        }
    }

    private sealed class StaticHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new(handler, disposeHandler: false)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }
    }

    private sealed class CountingHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handleAsync) : HttpMessageHandler
    {
        private int requestCount;

        public int RequestCount => Volatile.Read(ref requestCount);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref requestCount);
            var response = await handleAsync(request);
            response.RequestMessage = request;
            return response;
        }
    }
}
