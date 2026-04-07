using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.RefreshAllowedTenantCache;

public sealed class TenantsRefreshAllowedTenantCacheCommand : IRequest<bool>
{
    public required Guid TenantId { get; init; }
}
