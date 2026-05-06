using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.RefreshAllowedTenantCache;

public sealed class TenantsRefreshAllowedTenantCacheCommand : IRequest<bool>
{
}
