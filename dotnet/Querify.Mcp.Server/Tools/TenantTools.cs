using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using Querify.Mcp.Common.Constants;
using Querify.Mcp.Server.Infrastructure;
using Querify.Tenant.Portal.Business.Billing.Queries.GetBillingSubscription;
using Querify.Tenant.Portal.Business.Billing.Queries.GetBillingSummary;
using Querify.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;
using Querify.Tenant.Portal.Business.Tenant.Queries.GetClientKey;
using Querify.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;
using Querify.Tenant.Portal.Business.User.Queries.GetUserProfile;

namespace Querify.Mcp.Server.Tools;

[McpServerToolType]
public sealed class TenantTools(
    IMediator mediator,
    McpRequestContext requestContext)
{
    [McpServerTool(
        Name = McpToolNames.TenantListWorkspaces,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads workspaces available to the configured MCP service user through the Tenant query boundary.")]
    public Task<string> ListWorkspaces(
        [Description("Tenant id used to establish MCP session context. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new TenantsGetAllTenantsQuery(), cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.TenantGetClientKey,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads a workspace client key through the Tenant query boundary. This tool never generates or rotates keys.")]
    public Task<string> GetClientKey(
        [Description("Tenant id.")]
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new TenantsGetClientKeyQuery { TenantId = tenantId }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.TenantListMembers,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads workspace members through the Tenant query boundary.")]
    public Task<string> ListMembers(
        [Description("Tenant id.")]
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new TenantUsersGetTenantUserListQuery { TenantId = tenantId }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.TenantGetProfile,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads the configured MCP service user's profile through the Tenant User query boundary.")]
    public Task<string> GetProfile(
        [Description("Tenant id used to establish MCP session context. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new UsersGetUserProfileQuery(), cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.TenantGetBillingSummary,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads workspace billing summary through the Tenant Billing query boundary.")]
    public Task<string> GetBillingSummary(
        [Description("Tenant id.")]
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new GetBillingSummaryQuery { TenantId = tenantId }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.TenantGetSubscription,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads workspace subscription details through the Tenant Billing query boundary.")]
    public Task<string> GetSubscription(
        [Description("Tenant id.")]
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new GetBillingSubscriptionQuery { TenantId = tenantId }, cancellationToken));
    }

    private Task<string> ExecuteAsync(Guid? tenantId, Func<Task<object?>> action)
    {
        return McpToolExecution.ExecuteAsync(async () =>
        {
            requestContext.Configure(tenantId);
            return await action();
        });
    }
}
