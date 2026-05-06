using Querify.QnA.Common.Domain.Entities;

namespace Querify.QnA.Common.Domain.BusinessRules.Activities;

public static class ActivityEntityMetadata
{
    public static Dictionary<string, object?> SnapshotQuestion(Question entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Id"] = entity.Id,
            ["TenantId"] = entity.TenantId,
            ["SpaceId"] = entity.SpaceId,
            ["Title"] = entity.Title,
            ["Summary"] = entity.Summary,
            ["ContextNote"] = entity.ContextNote,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString(),
            ["OriginChannel"] = entity.OriginChannel.ToString(),
            ["AiConfidenceScore"] = entity.AiConfidenceScore,
            ["FeedbackScore"] = entity.FeedbackScore,
            ["Sort"] = entity.Sort,
            ["AcceptedAnswerId"] = entity.AcceptedAnswerId
        };
    }

    public static Dictionary<string, object?> QuestionContext(Question entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["QuestionId"] = entity.Id,
            ["SpaceId"] = entity.SpaceId,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString()
        };
    }

    public static Dictionary<string, object?> SnapshotAnswer(Answer entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Id"] = entity.Id,
            ["TenantId"] = entity.TenantId,
            ["QuestionId"] = entity.QuestionId,
            ["Headline"] = entity.Headline,
            ["Body"] = entity.Body,
            ["AuthorLabel"] = entity.AuthorLabel,
            ["ContextNote"] = entity.ContextNote,
            ["Kind"] = entity.Kind.ToString(),
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString(),
            ["AiConfidenceScore"] = entity.AiConfidenceScore,
            ["Score"] = entity.Score,
            ["Sort"] = entity.Sort
        };
    }

    public static Dictionary<string, object?> AnswerContext(Answer entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["QuestionId"] = entity.QuestionId,
            ["AnswerId"] = entity.Id,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString()
        };
    }

    public static ActivitySignalEntry ToSignalEntry(Activity entity)
    {
        return new ActivitySignalEntry(
            entity.Kind,
            entity.AnswerId,
            entity.OccurredAtUtc,
            entity.UserPrint,
            entity.MetadataJson);
    }
}
