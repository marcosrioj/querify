using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Attributes;
using Querify.Common.Infrastructure.Signalr.Portal.Models;
using Querify.Common.Infrastructure.Signalr.Portal.Options;

namespace Querify.Common.Infrastructure.Signalr.Portal.Hubs;

[Authorize]
[SkipTenantAccessValidation]
public sealed class PortalNotificationsHub(
    ISessionService sessionService,
    IAllowedTenantStore allowedTenantStore,
    IAllowedTenantProvider allowedTenantProvider,
    IOptions<PortalSignalROptions> options)
    : Hub
{
    public const string Path = PortalSignalROptions.DefaultNotificationsHubPath;

    public override async Task OnConnectedAsync()
    {
        var tenantId = ResolveTenantId();
        var userId = sessionService.GetUserId();

        if (!await IsTenantAllowedAsync(userId, tenantId, Context.ConnectionAborted))
        {
            Context.Abort();
            throw new HubException("Tenant is not allowed for the current user.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            PortalNotificationGroups.TenantModule(tenantId, options.Value.Module),
            Context.ConnectionAborted);
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            PortalNotificationGroups.User(userId),
            Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    private Guid ResolveTenantId()
    {
        var rawTenantId = Context.GetHttpContext()?.Request.Query["tenantId"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawTenantId) || !Guid.TryParse(rawTenantId, out var tenantId))
        {
            Context.Abort();
            throw new HubException("A valid tenantId query parameter is required.");
        }

        return tenantId;
    }

    private async Task<bool> IsTenantAllowedAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var allowedTenants = await allowedTenantStore.GetAllowedTenantIds(userId, cancellationToken);
        if (allowedTenants is null)
        {
            allowedTenants = await allowedTenantProvider.GetAllowedTenantIds(userId, cancellationToken);
            await allowedTenantStore.SetAllowedTenantIds(
                userId,
                allowedTenants,
                ttl: null,
                cancellationToken: cancellationToken);
        }

        return allowedTenants.TryGetValue(options.Value.Module.ToString(), out var tenantIds) &&
            tenantIds.Contains(tenantId);
    }
}
