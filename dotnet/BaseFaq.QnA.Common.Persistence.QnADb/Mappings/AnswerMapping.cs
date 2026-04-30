using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Mappings;

public static class AnswerMapping
{
    public static AnswerDto ToPortalAnswerDto(
        this Answer entity,
        IEnumerable<Activity> questionActivity,
        Guid? acceptedAnswerId)
    {
        return ToAnswerDto(entity, questionActivity, acceptedAnswerId, false);
    }

    public static AnswerDto ToPublicAnswerDto(
        this Answer entity,
        IEnumerable<Activity> questionActivity,
        Guid? acceptedAnswerId)
    {
        return ToAnswerDto(entity, questionActivity, acceptedAnswerId, true);
    }

    private static AnswerDto ToAnswerDto(
        Answer entity,
        IEnumerable<Activity> questionActivity,
        Guid? acceptedAnswerId,
        bool publicOnly)
    {
        IEnumerable<AnswerSourceLink> sources = entity.Sources;
        if (publicOnly)
            sources = sources.Where(link =>
                link.Source is not null &&
                link.Source.Visibility is VisibilityScope.Public);

        return new AnswerDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            QuestionId = entity.QuestionId,
            Headline = entity.Headline,
            Body = entity.Body,
            Kind = entity.Kind,
            Status = entity.Status,
            Visibility = entity.Visibility,
            ContextNote = entity.ContextNote,
            AuthorLabel = entity.AuthorLabel,
            AiConfidenceScore = entity.AiConfidenceScore,
            Score = entity.Score,
            Sort = entity.Sort,
            IsAccepted = entity.Id == acceptedAnswerId,
            IsOfficial = entity.Kind == AnswerKind.Official,
            LastUpdatedAtUtc = entity.UpdatedDate ?? entity.CreatedDate,
            VoteScore = ActivitySignals.ComputeVoteScore(questionActivity.Select(ToSignalEntry), entity.Id),
            Sources = sources
                .OrderBy(source => source.Order)
                .Select(source => source.ToAnswerSourceLinkDto())
                .ToList()
        };
    }

    private static ActivitySignalEntry ToSignalEntry(Activity entity)
    {
        return new ActivitySignalEntry(
            entity.Kind,
            entity.AnswerId,
            entity.OccurredAtUtc,
            entity.UserPrint,
            entity.MetadataJson);
    }
}
