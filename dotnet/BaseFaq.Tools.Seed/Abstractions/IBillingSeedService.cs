using BaseFaq.Common.EntityFramework.Tenant;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface IBillingSeedService
{
    bool HasBillingData(TenantDbContext dbContext, Guid seedTenantId);
    void SeedBillingData(TenantDbContext dbContext, Guid seedTenantId, string faqConnectionString);
}
