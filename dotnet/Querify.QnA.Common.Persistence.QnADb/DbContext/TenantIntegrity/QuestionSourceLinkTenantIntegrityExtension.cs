using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class QuestionSourceLinkTenantIntegrityExtension
{
    internal static void EnsureQuestionSourceLinkTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<QuestionSourceLink>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetQuestionTenant(link.QuestionId),
                nameof(QuestionSourceLink.QuestionId));
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetSourceTenant(link.SourceId),
                nameof(QuestionSourceLink.SourceId));
        }
    }
}
