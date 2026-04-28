using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.Tools.Seed.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tools.Seed.Application;

public sealed class CleanupService : ICleanupService
{
    public void CleanTenantDb(TenantDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            "TRUNCATE TABLE \"BillingWebhookInboxes\", \"EmailOutboxes\", \"TenantConnections\", \"TenantUsers\", \"Tenants\", \"Users\" RESTART IDENTITY CASCADE;");
    }

    public void CleanQnADb(QnADbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            "TRUNCATE TABLE \"Activities\", \"AnswerSourceLinks\", \"QuestionSourceLinks\", \"QuestionTags\", \"SpaceSources\", \"SpaceTags\", \"Answers\", \"Questions\", \"Sources\", \"Tags\", \"Spaces\" RESTART IDENTITY CASCADE;");
    }
}
