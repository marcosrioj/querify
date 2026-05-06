using EntityTenant = Querify.Common.EntityFramework.Tenant.Entities.Tenant;

namespace Querify.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantPortalAccessService
{
    Task EnsureAccessAsync(Guid tenantId, CancellationToken cancellationToken);
    Task EnsureOwnerAccessAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<EntityTenant> GetAccessibleTenantAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<EntityTenant> GetAccessibleTenantWithUsersAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<EntityTenant> GetOwnedTenantWithUsersAsync(Guid tenantId, CancellationToken cancellationToken);
}
