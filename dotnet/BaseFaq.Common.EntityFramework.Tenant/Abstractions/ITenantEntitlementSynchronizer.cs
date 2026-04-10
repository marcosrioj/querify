using BaseFaq.Common.EntityFramework.Tenant.Entities;

namespace BaseFaq.Common.EntityFramework.Tenant.Abstractions;

public interface ITenantEntitlementSynchronizer
{
    Task<TenantEntitlementSnapshot> SynchronizeAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
