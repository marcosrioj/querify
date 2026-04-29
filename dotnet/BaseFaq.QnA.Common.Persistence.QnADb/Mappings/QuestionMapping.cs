using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Mappings;

public static class QuestionMapping
{
    public static QuestionDto ToQuestionDto(this Question entity)
    {
        return new QuestionDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SpaceId = entity.SpaceId,
            SpaceSlug = entity.Space.Slug,
            Title = entity.Title,
            Summary = entity.Summary,
            ContextNote = entity.ContextNote,
            Status = entity.Status,
            Visibility = entity.Visibility,
            OriginChannel = entity.OriginChannel,
            AiConfidenceScore = entity.AiConfidenceScore,
            FeedbackScore = entity.FeedbackScore,
            Sort = entity.Sort,
            AcceptedAnswerId = entity.AcceptedAnswerId,
            LastActivityAtUtc = entity.LastActivityAtUtc
        };
    }

    public static QuestionDetailDto ToPortalQuestionDetailDto(this Question entity)
    {
        return new QuestionDetailDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SpaceId = entity.SpaceId,
            SpaceSlug = entity.Space.Slug,
            Title = entity.Title,
            Summary = entity.Summary,
            ContextNote = entity.ContextNote,
            Status = entity.Status,
            Visibility = entity.Visibility,
            OriginChannel = entity.OriginChannel,
            AiConfidenceScore = entity.AiConfidenceScore,
            FeedbackScore = entity.FeedbackScore,
            Sort = entity.Sort,
            AcceptedAnswerId = entity.AcceptedAnswerId,
            LastActivityAtUtc = entity.LastActivityAtUtc,
            AcceptedAnswer = entity.AcceptedAnswer?.ToPortalAnswerDto(entity.Activities, entity.AcceptedAnswerId),
            Answers = entity.Answers
                .OrderByDescending(answer => answer.Id == entity.AcceptedAnswerId)
                .ThenBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.Score)
                .ThenBy(answer => answer.Headline)
                .Select(answer => answer.ToPortalAnswerDto(entity.Activities, entity.AcceptedAnswerId))
                .ToList(),
            Tags = entity.Tags.Select(link => link.Tag.ToTagDto()).ToList(),
            Sources = entity.Sources
                .OrderBy(source => source.Order)
                .Select(source => source.ToQuestionSourceLinkDto())
                .ToList(),
            Activity = entity.Activities
                .OrderByDescending(activity => activity.OccurredAtUtc)
                .Select(activity => activity.ToActivityDto())
                .ToList()
        };
    }

    public static QuestionDetailDto ToPublicQuestionDetailDto(
        this Question entity,
        QuestionGetRequestDto request)
    {
        var publicSources = entity.Sources
            .Where(link =>
                link.Source is not null &&
                link.Source.Visibility is VisibilityScope.Public)
            .OrderBy(source => source.Order)
            .Select(source => source.ToQuestionSourceLinkDto())
            .ToList();
        var publicAnswers = entity.Answers
            .Where(answer =>
                answer.Visibility is VisibilityScope.Public &&
                answer.Status is AnswerStatus.Active)
            .OrderByDescending(answer => answer.Id == entity.AcceptedAnswerId)
            .ThenBy(answer => answer.Sort)
            .ThenByDescending(answer => answer.Score)
            .ThenBy(answer => answer.Headline)
            .Select(answer => answer.ToPublicAnswerDto(entity.Activities, entity.AcceptedAnswerId))
            .ToList();
        var acceptedAnswer = publicAnswers.FirstOrDefault(answer => answer.Id == entity.AcceptedAnswerId);

        return new QuestionDetailDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SpaceId = entity.SpaceId,
            SpaceSlug = entity.Space.Slug,
            Title = entity.Title,
            Summary = entity.Summary,
            ContextNote = entity.ContextNote,
            Status = entity.Status,
            Visibility = entity.Visibility,
            OriginChannel = entity.OriginChannel,
            AiConfidenceScore = entity.AiConfidenceScore,
            FeedbackScore = entity.FeedbackScore,
            Sort = entity.Sort,
            AcceptedAnswerId = acceptedAnswer?.Id,
            LastActivityAtUtc = entity.LastActivityAtUtc,
            AcceptedAnswer = request.IncludeAnswers ? acceptedAnswer : null,
            Answers = request.IncludeAnswers ? publicAnswers : [],
            Tags = request.IncludeTags
                ? entity.Tags.Select(link => link.Tag.ToTagDto()).ToList()
                : [],
            Sources = request.IncludeSources ? publicSources : [],
            Activity = request.IncludeActivity
                ? entity.Activities
                    .OrderByDescending(activity => activity.OccurredAtUtc)
                    .Select(activity => activity.ToActivityDto())
                    .ToList()
                : []
        };
    }
}
