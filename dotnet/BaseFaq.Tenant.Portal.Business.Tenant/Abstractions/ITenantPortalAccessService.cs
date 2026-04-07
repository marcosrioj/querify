using EntityTenant = BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantPortalAccessService
{
    Task EnsureAccessAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<EntityTenant> GetAccessibleTenantAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<EntityTenant> GetAccessibleTenantWithUsersAsync(Guid tenantId, CancellationToken cancellationToken);
}
