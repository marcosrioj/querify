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
        var userId = sessionService.GetUserId();
        var allowedTenants = await GetAllowedTenantIdsAsync(
            userId,
            Context.ConnectionAborted);
        var moduleKey = options.Value.Module.ToString();
        allowedTenants.TryGetValue(moduleKey, out var moduleTenantIds);
        moduleTenantIds ??= [];

        var requestedTenantId = ResolveTenantId();
        if (requestedTenantId is not null && !moduleTenantIds.Contains(requestedTenantId.Value))
        {
            Context.Abort();
            throw new HubException("Tenant is not allowed for the current user.");
        }

        IEnumerable<Guid> connectionTenantIds =
            requestedTenantId is Guid scopedTenantId ? [scopedTenantId] : moduleTenantIds;

        foreach (var tenantId in connectionTenantIds)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                PortalNotificationGroups.TenantModule(tenantId, options.Value.Module),
                Context.ConnectionAborted);
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            PortalNotificationGroups.User(userId),
            Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    private Guid? ResolveTenantId()
    {
        var rawTenantId = Context.GetHttpContext()?.Request.Query["tenantId"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawTenantId))
        {
            return null;
        }

        if (!Guid.TryParse(rawTenantId, out var tenantId))
        {
            Context.Abort();
            throw new HubException("The tenantId query parameter must be a valid tenant id.");
        }

        return tenantId;
    }

    private async Task<IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>> GetAllowedTenantIdsAsync(
        Guid userId,
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

        return allowedTenants;
    }
}
