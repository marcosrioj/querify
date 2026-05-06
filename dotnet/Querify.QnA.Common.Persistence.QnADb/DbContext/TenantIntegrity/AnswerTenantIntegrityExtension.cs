using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

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
                throw new ApiErrorException(
                    $"Answer '{answer.Id}' cannot be public while in status '{answer.Status}'.",
                    (int)HttpStatusCode.UnprocessableEntity);
        }
    }
}
