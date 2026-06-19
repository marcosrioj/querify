using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Mcp.Server.Options;

namespace Querify.Mcp.Server.Infrastructure;

public sealed class McpRequestContext(
    McpSessionContext sessionContext,
    IHttpContextAccessor httpContextAccessor,
    IOptions<McpServerOptions> options)
{
    public void Configure(Guid? tenantId)
    {
        var currentOptions = options.Value;
        var resolvedTenantId = tenantId ?? currentOptions.DefaultTenantId;

        if (resolvedTenantId is null || resolvedTenantId == Guid.Empty)
        {
            throw new ApiErrorException(
                "MCP tenantId is required. Pass tenantId in the tool call or configure McpServer:DefaultTenantId.",
                (int)HttpStatusCode.BadRequest);
        }

        sessionContext.Set(
            resolvedTenantId.Value,
            currentOptions.ServiceUserId,
            currentOptions.ServiceUserName);

        EnsureHttpContext();
    }

    private void EnsureHttpContext()
    {
        var httpContext = httpContextAccessor.HttpContext ?? new DefaultHttpContext();

        httpContext.Connection.RemoteIpAddress ??= IPAddress.Loopback;
        httpContext.Request.Headers.UserAgent = "Querify.Mcp.Server";
        httpContext.Request.Headers["X-Forwarded-For"] = IPAddress.Loopback.ToString();

        httpContextAccessor.HttpContext = httpContext;
    }
}
