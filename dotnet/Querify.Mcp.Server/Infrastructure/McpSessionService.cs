using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;

namespace Querify.Mcp.Server.Infrastructure;

public sealed class McpSessionService(McpSessionContext context) : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        if (!context.IsConfigured)
        {
            throw new ApiErrorException(
                "MCP tenant context is not configured for the current tool request.",
                (int)HttpStatusCode.BadRequest);
        }

        return module is ModuleEnum.Tenant or ModuleEnum.QnA
            ? context.TenantId
            : throw new ApiErrorException(
                $"MCP Stage 1 cannot resolve tenant context for module '{module}'.",
                (int)HttpStatusCode.InternalServerError);
    }

    public Guid GetUserId()
    {
        if (!context.IsConfigured)
        {
            throw new ApiErrorException(
                "MCP service user context is not configured for the current tool request.",
                (int)HttpStatusCode.BadRequest);
        }

        return context.UserId;
    }

    public string? GetUserName()
    {
        return context.IsConfigured ? context.UserName : null;
    }
}
