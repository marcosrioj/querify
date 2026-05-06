using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class QuestionTagTenantIntegrityExtension
{
    internal static void EnsureQuestionTagTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<QuestionTag>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetQuestionTenant(link.QuestionId),
                nameof(QuestionTag.QuestionId));
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetTagTenant(link.TagId),
                nameof(QuestionTag.TagId));
        }
    }
}
