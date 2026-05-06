namespace Querify.Common.EntityFramework.Core.Tenant.Abstractions;

public interface ITenantFilterState
{
    Guid? SessionTenantId { get; }
    bool TenantFiltersEnabled { get; }
}
