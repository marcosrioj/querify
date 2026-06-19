using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;

namespace Querify.Mcp.Server.Infrastructure;

public sealed class McpSessionContext
{
    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string? UserName { get; private set; }

    public bool IsConfigured { get; private set; }

    public void Set(Guid tenantId, Guid userId, string? userName)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ApiErrorException(
                "MCP tenant context requires a non-empty tenantId.",
                (int)HttpStatusCode.BadRequest);
        }

        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "MCP service user context is missing. Configure McpServer:ServiceUserId before using tools.");
        }

        TenantId = tenantId;
        UserId = userId;
        UserName = userName;
        IsConfigured = true;
    }
}
