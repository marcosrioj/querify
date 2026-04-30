using BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class ActivityTenantIntegrityExtension
{
    internal static void EnsureActivityTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<Activity>()
                     .Where(entry => entry.State != EntityState.Unchanged))
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new ApiErrorException(
                    $"Activity '{entry.Entity.Id}' is append-only and cannot be modified or deleted.",
                    (int)HttpStatusCode.UnprocessableEntity);

            var activity = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                activity.TenantId,
                cache.GetQuestionTenant(activity.QuestionId),
                nameof(Activity.QuestionId));

            if (activity.AnswerId is not Guid answerId) continue;

            var answer = cache.GetAnswer(answerId);
            TenantIntegrityGuard.EnsureTenantMatch(activity.TenantId, answer.TenantId, nameof(Activity.AnswerId));

            if (answer.QuestionId != activity.QuestionId)
                throw new ApiErrorException(
                    $"Activity '{activity.Id}' references answer '{answerId}' from a different question.",
                    (int)HttpStatusCode.UnprocessableEntity);
        }
    }
}
