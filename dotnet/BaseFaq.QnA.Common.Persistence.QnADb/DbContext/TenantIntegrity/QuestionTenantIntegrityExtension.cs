using BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class QuestionTenantIntegrityExtension
{
    internal static void EnsureQuestionTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<Question>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var question = entry.Entity;

            TenantIntegrityGuard.EnsureTenantMatch(
                question.TenantId,
                cache.GetSpaceTenant(question.SpaceId),
                nameof(Question.SpaceId));

            if (question.Visibility is VisibilityScope.Public &&
                question.Status is not QuestionStatus.Active)
                throw new InvalidOperationException(
                    $"Question '{question.Id}' cannot be public while in status '{question.Status}'.");

            if (question.Visibility is VisibilityScope.Public &&
                question.DuplicateOfQuestionId.HasValue)
                throw new InvalidOperationException(
                    $"Question '{question.Id}' cannot be public while it points to a duplicate target.");

            if (question.AcceptedAnswerId is Guid acceptedAnswerId)
            {
                var acceptedAnswer = cache.GetAnswer(acceptedAnswerId);
                TenantIntegrityGuard.EnsureTenantMatch(
                    question.TenantId,
                    acceptedAnswer.TenantId,
                    nameof(Question.AcceptedAnswerId));

                if (acceptedAnswer.QuestionId != question.Id)
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' accepts answer '{acceptedAnswerId}' from a different question.");

                if (acceptedAnswer.Status is not AnswerStatus.Active)
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' cannot accept answer '{acceptedAnswerId}' while it is in status '{acceptedAnswer.Status}'.");

                if (question.Visibility is VisibilityScope.Public &&
                    acceptedAnswer.Visibility is not VisibilityScope.Public)
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' cannot expose accepted answer '{acceptedAnswerId}' while the answer is not publicly visible.");
            }

            if (question.DuplicateOfQuestionId is Guid duplicateQuestionId)
            {
                var duplicateOfQuestionTenantId = cache.GetQuestionTenant(duplicateQuestionId);
                TenantIntegrityGuard.EnsureTenantMatch(
                    question.TenantId,
                    duplicateOfQuestionTenantId,
                    nameof(Question.DuplicateOfQuestionId));

                if (duplicateQuestionId == question.Id)
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' cannot point to itself as a duplicate.");
            }
        }
    }
}
