using BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class AnswerTenantIntegrityExtension
{
    internal static void EnsureAnswerTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<Answer>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var answer = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                answer.TenantId,
                cache.GetQuestionTenant(answer.QuestionId),
                nameof(Answer.QuestionId));

            if (answer.Visibility is VisibilityScope.Public &&
                answer.Status is not AnswerStatus.Active)
                throw new InvalidOperationException(
                    $"Answer '{answer.Id}' cannot be public while in status '{answer.Status}'.");
        }
    }
}
