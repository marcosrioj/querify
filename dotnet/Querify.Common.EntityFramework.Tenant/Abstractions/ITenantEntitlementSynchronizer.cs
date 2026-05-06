using Querify.Common.EntityFramework.Tenant.Entities;

namespace Querify.Common.EntityFramework.Tenant.Abstractions;

public interface ITenantEntitlementSynchronizer
{
    Task<TenantEntitlementSnapshot> SynchronizeAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
