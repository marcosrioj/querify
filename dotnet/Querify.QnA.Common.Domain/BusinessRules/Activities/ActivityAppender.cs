using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;

namespace Querify.QnA.Common.Domain.BusinessRules.Activities;

public static class ActivityAppender
{
    public static Activity? AddQuestionActivity(
        Question question,
        ActivityKind kind,
        ActivityActor actor,
        string operation,
        IReadOnlyDictionary<string, object?> before,
        IReadOnlyDictionary<string, object?> after,
        IReadOnlyDictionary<string, object?>? context = null,
        DateTime? occurredAtUtc = null)
    {
        var metadataJson = ActivityChangeMetadata.Create(
            "Question",
            operation,
            question.Id,
            before,
            after,
            MergeContext(context, actor),
            maxLength: Activity.MaxMetadataLength);

        if (metadataJson is null)
            return null;

        var activity = CreateActivity(
            question,
            null,
            kind,
            actor,
            metadataJson,
            CreateNote(kind, actor),
            occurredAtUtc);

        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;

        return activity;
    }

    public static Activity? AddAnswerActivity(
        Answer answer,
        ActivityKind kind,
        ActivityActor actor,
        string operation,
        IReadOnlyDictionary<string, object?> before,
        IReadOnlyDictionary<string, object?> after,
        IReadOnlyDictionary<string, object?>? context = null,
        DateTime? occurredAtUtc = null)
    {
        var metadataJson = ActivityChangeMetadata.Create(
            "Answer",
            operation,
            answer.Id,
            before,
            after,
            MergeContext(context, actor),
            maxLength: Activity.MaxMetadataLength);

        if (metadataJson is null)
            return null;

        var activity = CreateActivity(
            answer.Question,
            answer,
            kind,
            actor,
            metadataJson,
            CreateNote(kind, actor),
            occurredAtUtc);

        answer.Question.Activities.Add(activity);
        answer.Question.LastActivityAtUtc = activity.OccurredAtUtc;

        return activity;
    }

    public static Activity AddFeedbackActivity(
        Question question,
        ActivityActor actor,
        bool like,
        string? reason,
        string? notes,
        DateTime? occurredAtUtc = null)
    {
        var activity = CreateActivity(
            question,
            null,
            ActivityKind.FeedbackReceived,
            actor,
            ActivitySignals.CreateFeedbackMetadata(
                actor.UserPrint,
                actor.Ip,
                actor.UserAgent,
                like,
                reason,
                actor.AuditUserId,
                actor.DisplayName,
                ActorSource(actor)),
            CreateFeedbackNote(actor, like, notes),
            occurredAtUtc);

        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;

        return activity;
    }

    public static Activity AddVoteActivity(
        Answer answer,
        ActivityActor actor,
        int voteValue,
        string? notes,
        DateTime? occurredAtUtc = null)
    {
        var activity = CreateActivity(
            answer.Question,
            answer,
            ActivityKind.VoteReceived,
            actor,
            ActivitySignals.CreateVoteMetadata(
                actor.UserPrint,
                actor.Ip,
                actor.UserAgent,
                voteValue,
                actor.AuditUserId,
                actor.DisplayName,
                ActorSource(actor)),
            CreateVoteNote(actor, voteValue, notes),
            occurredAtUtc);

        answer.Question.Activities.Add(activity);
        answer.Question.LastActivityAtUtc = activity.OccurredAtUtc;

        return activity;
    }

    private static Activity CreateActivity(
        Question question,
        Answer? answer,
        ActivityKind kind,
        ActivityActor actor,
        string metadataJson,
        string notes,
        DateTime? occurredAtUtc)
    {
        return new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer?.Id,
            Answer = answer,
            Kind = kind,
            ActorKind = actor.ActorKind,
            ActorLabel = actor.ActorLabel,
            UserPrint = actor.UserPrint,
            Ip = actor.Ip,
            UserAgent = actor.UserAgent,
            Notes = notes,
            MetadataJson = metadataJson,
            OccurredAtUtc = occurredAtUtc ?? DateTime.UtcNow,
            CreatedBy = actor.AuditUserId,
            UpdatedBy = actor.AuditUserId
        };
    }

    private static Dictionary<string, object?> MergeContext(
        IReadOnlyDictionary<string, object?>? context,
        ActivityActor actor)
    {
        var merged = context is null
            ? new Dictionary<string, object?>(StringComparer.Ordinal)
            : new Dictionary<string, object?>(context, StringComparer.Ordinal);

        merged["ActorKind"] = actor.ActorKind.ToString();
        merged["ActorUserPrint"] = actor.UserPrint;
        merged["ActorUserId"] = actor.AuditUserId;
        merged["ActorUserName"] = actor.DisplayName;
        merged["ActorSource"] = ActorSource(actor);

        return merged;
    }

    private static string CreateNote(ActivityKind kind, ActivityActor actor)
    {
        var actorName = actor.DisplayName;
        return kind switch
        {
            ActivityKind.QuestionCreated => $"{actorName} created the question.",
            ActivityKind.QuestionUpdated => $"{actorName} updated the question.",
            ActivityKind.QuestionDraft => $"{actorName} changed the question status to Draft.",
            ActivityKind.QuestionActive => $"{actorName} changed the question status to Active.",
            ActivityKind.QuestionArchived => $"{actorName} changed the question status to Archived.",
            ActivityKind.AnswerCreated => $"{actorName} created the answer.",
            ActivityKind.AnswerUpdated => $"{actorName} updated the answer.",
            ActivityKind.AnswerDraft => $"{actorName} changed the answer status to Draft.",
            ActivityKind.AnswerActive => $"{actorName} changed the answer status to Active.",
            ActivityKind.AnswerArchived => $"{actorName} changed the answer status to Archived.",
            ActivityKind.FeedbackReceived => $"{actorName} submitted question feedback.",
            ActivityKind.VoteReceived => $"{actorName} submitted an answer vote.",
            _ => $"{actorName} recorded activity."
        };
    }

    private static string CreateFeedbackNote(ActivityActor actor, bool like, string? notes)
    {
        var summary = like
            ? $"{actor.DisplayName} submitted positive feedback for the question."
            : $"{actor.DisplayName} submitted negative feedback for the question.";

        return string.IsNullOrWhiteSpace(notes)
            ? summary
            : $"{summary} Notes: {notes}";
    }

    private static string CreateVoteNote(ActivityActor actor, int voteValue, string? notes)
    {
        var summary = voteValue switch
        {
            > 0 => $"{actor.DisplayName} upvoted the answer.",
            < 0 => $"{actor.DisplayName} downvoted the answer.",
            _ => $"{actor.DisplayName} cleared their answer vote."
        };

        return string.IsNullOrWhiteSpace(notes)
            ? summary
            : $"{summary} Notes: {notes}";
    }

    private static string ActorSource(ActivityActor actor)
    {
        return actor.IsPublic ? "Public" : "Portal";
    }
}
