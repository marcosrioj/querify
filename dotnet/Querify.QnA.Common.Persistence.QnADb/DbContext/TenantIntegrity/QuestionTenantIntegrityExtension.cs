using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

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
                throw new ApiErrorException(
                    $"Question '{question.Id}' cannot be public while in status '{question.Status}'.",
                    (int)HttpStatusCode.UnprocessableEntity);

            if (question.ParentAnswerId is Guid parentAnswerId)
            {
                var parentAnswer = cache.GetAnswer(parentAnswerId);
                TenantIntegrityGuard.EnsureTenantMatch(
                    question.TenantId,
                    parentAnswer.TenantId,
                    nameof(Question.ParentAnswerId));

                EnsureNoRecursiveParentAnswerCycle(cache, question, parentAnswer);
            }

            if (question.AcceptedAnswerId is Guid acceptedAnswerId)
            {
                var acceptedAnswer = cache.GetAnswer(acceptedAnswerId);
                TenantIntegrityGuard.EnsureTenantMatch(
                    question.TenantId,
                    acceptedAnswer.TenantId,
                    nameof(Question.AcceptedAnswerId));

                if (acceptedAnswer.QuestionId != question.Id)
                    throw new ApiErrorException(
                        $"Question '{question.Id}' accepts answer '{acceptedAnswerId}' from a different question.",
                        (int)HttpStatusCode.UnprocessableEntity);

                if (acceptedAnswer.Status is not AnswerStatus.Active)
                    throw new ApiErrorException(
                        $"Question '{question.Id}' cannot accept answer '{acceptedAnswerId}' while it is in status '{acceptedAnswer.Status}'.",
                        (int)HttpStatusCode.UnprocessableEntity);

                if (question.Visibility is VisibilityScope.Public &&
                    acceptedAnswer.Visibility is not VisibilityScope.Public)
                    throw new ApiErrorException(
                        $"Question '{question.Id}' cannot expose accepted answer '{acceptedAnswerId}' while the answer is not publicly visible.",
                        (int)HttpStatusCode.UnprocessableEntity);
            }

        }
    }

    private static void EnsureNoRecursiveParentAnswerCycle(
        TenantIntegrityLookupCache cache,
        Question question,
        AnswerTenantIntegrityLookup parentAnswer)
    {
        var visitedQuestionIds = new HashSet<Guid> { question.Id };
        var cursorQuestionId = parentAnswer.QuestionId;

        while (true)
        {
            if (!visitedQuestionIds.Add(cursorQuestionId))
                throw new ApiErrorException(
                    $"Question '{question.Id}' cannot use parent answer '{question.ParentAnswerId}' because it creates a recursive question-answer loop.",
                    (int)HttpStatusCode.UnprocessableEntity);

            var cursorQuestion = cache.GetQuestion(cursorQuestionId);
            if (cursorQuestion.ParentAnswerId is not Guid cursorParentAnswerId)
                return;

            cursorQuestionId = cache.GetAnswer(cursorParentAnswerId).QuestionId;
        }
    }
}
