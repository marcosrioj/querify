using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class AnswerSourceLinkTenantIntegrityExtension
{
    internal static void EnsureAnswerSourceLinkTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<AnswerSourceLink>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetAnswer(link.AnswerId).TenantId,
                nameof(AnswerSourceLink.AnswerId));
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetSourceTenant(link.SourceId),
                nameof(AnswerSourceLink.SourceId));
        }
    }
}