using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.RefreshAllowedTenantCache;

public sealed class TenantsRefreshAllowedTenantCacheCommandHandler(
    ISessionService sessionService,
    IAllowedTenantStore allowedTenantStore,
    IAllowedTenantProvider allowedTenantProvider,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantsRefreshAllowedTenantCacheCommand, bool>
{
    public async Task<bool> Handle(
        TenantsRefreshAllowedTenantCacheCommand request,
        CancellationToken cancellationToken)
    {
        await tenantPortalAccessService.EnsureAccessAsync(request.TenantId, cancellationToken);

        var userId = sessionService.GetUserId();
        var allowedTenants = await allowedTenantProvider.GetAllowedTenantIds(userId, cancellationToken);
        await allowedTenantStore.SetAllowedTenantIds(
            userId,
            allowedTenants,
            cancellationToken: cancellationToken);

        return true;
    }
}
