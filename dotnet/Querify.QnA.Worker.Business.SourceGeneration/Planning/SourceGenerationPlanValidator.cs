using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;

namespace Querify.QnA.Worker.Business.SourceGeneration.Planning;

internal static class SourceGenerationPlanValidator
{
    public static void Validate(SourceGenerationRun run, SourceGenerationPlan plan)
    {
        if (run.Visibility is VisibilityScope.Public)
            throw new ApiErrorException(
                "Generated content cannot be public.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (!Enum.IsDefined(run.SpaceStatus))
            throw new ApiErrorException(
                "Unsupported generated space status.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (plan.Questions.Count == 0)
            throw new ApiErrorException(
                "The generation plan did not include any questions.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (plan.Tags.Count > 12)
            throw new ApiErrorException(
                "The generation plan exceeded the tag limit.",
                (int)HttpStatusCode.UnprocessableEntity);

        var questionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var answerOwners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var questionParentAnswers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var question in plan.Questions)
        {
            if (string.IsNullOrWhiteSpace(question.TempId))
                throw new ApiErrorException(
                    "The generation plan included a question without a temporary id.",
                    (int)HttpStatusCode.UnprocessableEntity);

            if (!questionIds.Add(question.TempId))
                throw new ApiErrorException(
                    $"The generation plan included duplicate question id '{question.TempId}'.",
                    (int)HttpStatusCode.UnprocessableEntity);

            if (string.IsNullOrWhiteSpace(question.Title))
                throw new ApiErrorException(
                    "The generation plan included an empty question title.",
                    (int)HttpStatusCode.UnprocessableEntity);

            if (question.Answers.Count == 0)
                throw new ApiErrorException(
                    $"Question '{question.TempId}' did not include any answers.",
                    (int)HttpStatusCode.UnprocessableEntity);

            questionParentAnswers[question.TempId] = question.ParentAnswerTempId;

            foreach (var answer in question.Answers)
            {
                if (string.IsNullOrWhiteSpace(answer.TempId))
                    throw new ApiErrorException(
                        "The generation plan included an answer without a temporary id.",
                        (int)HttpStatusCode.UnprocessableEntity);

                if (!answerOwners.TryAdd(answer.TempId, question.TempId))
                    throw new ApiErrorException(
                        $"The generation plan included duplicate answer id '{answer.TempId}'.",
                        (int)HttpStatusCode.UnprocessableEntity);

                if (string.IsNullOrWhiteSpace(answer.Headline) || string.IsNullOrWhiteSpace(answer.Body))
                    throw new ApiErrorException(
                        $"Answer '{answer.TempId}' must include a headline and body.",
                        (int)HttpStatusCode.UnprocessableEntity);
            }
        }

        foreach (var question in plan.Questions.Where(question => question.ParentAnswerTempId is not null))
            if (!answerOwners.ContainsKey(question.ParentAnswerTempId!))
                throw new ApiErrorException(
                    $"Question '{question.TempId}' references missing parent answer '{question.ParentAnswerTempId}'.",
                    (int)HttpStatusCode.UnprocessableEntity);

        EnsureNoDuplicateSiblingTitles(plan, answerOwners);
        EnsureNoCycles(plan, answerOwners, questionParentAnswers);
    }

    private static void EnsureNoDuplicateSiblingTitles(
        SourceGenerationPlan plan,
        Dictionary<string, string> answerOwners)
    {
        var siblingGroups = plan.Questions
            .GroupBy(question => question.ParentAnswerTempId is null
                ? "root"
                : answerOwners[question.ParentAnswerTempId],
                StringComparer.OrdinalIgnoreCase);

        foreach (var group in siblingGroups)
        {
            var titles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var question in group)
            {
                var normalizedTitle = question.Title.Trim();
                if (!titles.Add(normalizedTitle))
                    throw new ApiErrorException(
                        $"The generation plan included duplicate sibling question title '{question.Title}'.",
                        (int)HttpStatusCode.UnprocessableEntity);
            }
        }
    }

    private static void EnsureNoCycles(
        SourceGenerationPlan plan,
        Dictionary<string, string> answerOwners,
        Dictionary<string, string?> questionParentAnswers)
    {
        foreach (var question in plan.Questions)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var current = question.TempId;

            while (questionParentAnswers.TryGetValue(current, out var parentAnswerId) &&
                   parentAnswerId is not null)
            {
                if (!seen.Add(current))
                    throw new ApiErrorException(
                        $"The generation plan included a follow-up cycle at question '{question.TempId}'.",
                        (int)HttpStatusCode.UnprocessableEntity);

                current = answerOwners[parentAnswerId];
            }
        }
    }
}
