using System.Net;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Session;

public static class IntegrationTestHttpContextFactory
{
    public static HttpContext Create(
        string userAgent,
        Action<DefaultHttpContext>? configure = null)
    {
        var httpContext = new DefaultHttpContext();
        ApplyDefaults(httpContext, userAgent);
        configure?.Invoke(httpContext);
        return httpContext;
    }

    public static HttpContext CreateWithEndpointMetadata(
        string userAgent,
        string displayName,
        params object[] metadata)
    {
        return Create(
            userAgent,
            httpContext =>
            {
                var endpoint = new Endpoint(
                    _ => Task.CompletedTask,
                    new EndpointMetadataCollection(metadata),
                    displayName);

                httpContext.SetEndpoint(endpoint);
            });
    }

    public static void ApplyDefaults(HttpContext httpContext, string userAgent)
    {
        httpContext.Connection.RemoteIpAddress ??= IPAddress.Loopback;

        if (string.IsNullOrWhiteSpace(httpContext.Request.Headers.UserAgent.ToString()))
        {
            httpContext.Request.Headers.UserAgent = userAgent;
        }
    }
}
