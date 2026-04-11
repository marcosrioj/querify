using BaseFaq.Common.EntityFramework.Tenant;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface IBillingSeedService
{
    bool HasBillingData(TenantDbContext dbContext);
    void SeedBillingData(TenantDbContext dbContext, string faqConnectionString);
}
