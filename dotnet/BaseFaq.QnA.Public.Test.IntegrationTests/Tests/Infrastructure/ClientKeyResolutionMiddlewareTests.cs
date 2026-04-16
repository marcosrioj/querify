using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Common.Infrastructure.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Infrastructure;

public class ClientKeyResolutionMiddlewareTests
{
    [Fact]
    public async Task Invoke_ThrowsWhenHeaderIsMissing()
    {
        var middleware = new ClientKeyResolutionMiddleware(_ => Task.CompletedTask);
        var httpContext = new DefaultHttpContext();

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => middleware.Invoke(httpContext));

        Assert.Equal(StatusCodes.Status400BadRequest, exception.ErrorCode);
    }

    [Fact]
    public async Task Invoke_ThrowsWhenHeaderIsEmpty()
    {
        var middleware = new ClientKeyResolutionMiddleware(_ => Task.CompletedTask);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[ClientKeyResolutionMiddleware.ClientKeyHeaderName] = "   ";

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => middleware.Invoke(httpContext));

        Assert.Equal(StatusCodes.Status400BadRequest, exception.ErrorCode);
    }

    [Fact]
    public async Task Invoke_StoresClientKeyOnly()
    {
        var nextCalled = false;
        var middleware = new ClientKeyResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[ClientKeyResolutionMiddleware.ClientKeyHeaderName] = "  public-key  ";

        await middleware.Invoke(httpContext);

        Assert.True(nextCalled);
        Assert.Equal("public-key", httpContext.Items[ClientKeyContextKeys.ClientKeyItemKey]);
        Assert.False(httpContext.Items.ContainsKey(TenantContextKeys.TenantIdItemKey));
    }
}
