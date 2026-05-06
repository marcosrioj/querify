using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.RefreshAllowedTenantCache;

public sealed class TenantsRefreshAllowedTenantCacheCommandHandler(
    ISessionService sessionService,
    IAllowedTenantStore allowedTenantStore,
    IAllowedTenantProvider allowedTenantProvider)
    : IRequestHandler<TenantsRefreshAllowedTenantCacheCommand, bool>
{
    public async Task<bool> Handle(
        TenantsRefreshAllowedTenantCacheCommand request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();
        var allowedTenants = await allowedTenantProvider.GetAllowedTenantIds(userId, cancellationToken);
        await allowedTenantStore.SetAllowedTenantIds(
            userId,
            allowedTenants,
            cancellationToken: cancellationToken);

        return true;
    }
}
