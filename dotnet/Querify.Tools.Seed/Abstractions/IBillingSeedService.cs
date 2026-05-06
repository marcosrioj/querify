using Querify.Common.EntityFramework.Tenant;

namespace Querify.Tools.Seed.Abstractions;

public interface IBillingSeedService
{
    bool HasBillingData(TenantDbContext dbContext, Guid seedTenantId);
    void SeedBillingData(TenantDbContext dbContext, Guid seedTenantId, string productConnectionString);
}
