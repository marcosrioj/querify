using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Tools.Seed.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tools.Seed.Application;

public sealed class CleanupService : ICleanupService
{
    public void CleanTenantDb(TenantDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            "TRUNCATE TABLE \"AiProviders\", \"TenantConnections\", \"Tenants\", \"Users\" RESTART IDENTITY CASCADE;");
    }

    public void CleanFaqDb(FaqDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            "TRUNCATE TABLE \"Votes\", \"FaqContentRefs\", \"FaqTags\", \"FaqItems\", \"ContentRefs\", \"Tags\", \"Faqs\" RESTART IDENTITY CASCADE;");
    }
}