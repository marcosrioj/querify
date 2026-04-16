using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Projections;

public static class QnAReadModelMappings
{
    public static QuestionDto ToQuestionDto(this Question entity)
    {
        return new QuestionDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SpaceId = entity.SpaceId,
            SpaceKey = entity.Space?.Key ?? string.Empty,
            Title = entity.Title,
            Key = entity.Key,
            Summary = entity.Summary,
            ContextNote = entity.ContextNote,
            Kind = entity.Kind,
            Status = entity.Status,
            Visibility = entity.Visibility,
            OriginChannel = entity.OriginChannel,
            Language = entity.Language,
            ProductScope = entity.ProductScope,
            JourneyScope = entity.JourneyScope,
            AudienceScope = entity.AudienceScope,
            ContextKey = entity.ContextKey,
            OriginUrl = entity.OriginUrl,
            OriginReference = entity.OriginReference,
            ThreadSummary = entity.ThreadSummary,
            ConfidenceScore = entity.ConfidenceScore,
            RevisionNumber = entity.RevisionNumber,
            AcceptedAnswerId = entity.AcceptedAnswerId,
            DuplicateOfQuestionId = entity.DuplicateOfQuestionId,
            AnsweredAtUtc = entity.AnsweredAtUtc,
            ResolvedAtUtc = entity.ResolvedAtUtc,
            ValidatedAtUtc = entity.ValidatedAtUtc,
            LastActivityAtUtc = entity.LastActivityAtUtc,
            FeedbackScore = ThreadActivitySignals.ComputeFeedbackScore(entity.Activities)
        };
    }

    public static QuestionDetailDto ToPortalQuestionDetailDto(this Question entity)
    {
        return new QuestionDetailDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SpaceId = entity.SpaceId,
            SpaceKey = entity.Space?.Key ?? string.Empty,
            Title = entity.Title,
            Key = entity.Key,
            Summary = entity.Summary,
            ContextNote = entity.ContextNote,
            Kind = entity.Kind,
            Status = entity.Status,
            Visibility = entity.Visibility,
            OriginChannel = entity.OriginChannel,
            Language = entity.Language,
            ProductScope = entity.ProductScope,
            JourneyScope = entity.JourneyScope,
            AudienceScope = entity.AudienceScope,
            ContextKey = entity.ContextKey,
            OriginUrl = entity.OriginUrl,
            OriginReference = entity.OriginReference,
            ThreadSummary = entity.ThreadSummary,
            ConfidenceScore = entity.ConfidenceScore,
            RevisionNumber = entity.RevisionNumber,
            AcceptedAnswerId = entity.AcceptedAnswerId,
            DuplicateOfQuestionId = entity.DuplicateOfQuestionId,
            AnsweredAtUtc = entity.AnsweredAtUtc,
            ResolvedAtUtc = entity.ResolvedAtUtc,
            ValidatedAtUtc = entity.ValidatedAtUtc,
            LastActivityAtUtc = entity.LastActivityAtUtc,
            FeedbackScore = ThreadActivitySignals.ComputeFeedbackScore(entity.Activities),
            AcceptedAnswer = entity.AcceptedAnswer?.ToPortalAnswerDto(entity.Activities, entity.AcceptedAnswerId),
            Answers = entity.Answers
                .OrderByDescending(answer => answer.Id == entity.AcceptedAnswerId)
                .ThenByDescending(answer => answer.Rank)
                .ThenBy(answer => answer.Headline)
                .Select(answer => answer.ToPortalAnswerDto(entity.Activities, entity.AcceptedAnswerId))
                .ToList(),
            Topics = entity.Topics.Select(link => link.Topic.ToTopicDto()).ToList(),
            Sources = entity.Sources
                .OrderBy(source => source.Order)
                .Select(source => source.ToQuestionSourceLinkDto())
                .ToList(),
            Activity = entity.Activities
                .OrderByDescending(activity => activity.OccurredAtUtc)
                .Select(activity => activity.ToThreadActivityDto())
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
                link.Source.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed)
            .OrderBy(source => source.Order)
            .Select(source => source.ToQuestionSourceLinkDto())
            .ToList();
        var publicAnswers = entity.Answers
            .Where(answer =>
                answer.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed &&
                answer.Status is AnswerStatus.Published or AnswerStatus.Validated)
            .OrderByDescending(answer => answer.Id == entity.AcceptedAnswerId)
            .ThenByDescending(answer => answer.Rank)
            .ThenBy(answer => answer.Headline)
            .Select(answer => answer.ToPublicAnswerDto(entity.Activities, entity.AcceptedAnswerId))
            .ToList();
        var acceptedAnswer = publicAnswers.FirstOrDefault(answer => answer.Id == entity.AcceptedAnswerId);

        return new QuestionDetailDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            SpaceId = entity.SpaceId,
            SpaceKey = entity.Space?.Key ?? string.Empty,
            Title = entity.Title,
            Key = entity.Key,
            Summary = entity.Summary,
            ContextNote = entity.ContextNote,
            Kind = entity.Kind,
            Status = entity.Status,
            Visibility = entity.Visibility,
            OriginChannel = entity.OriginChannel,
            Language = entity.Language,
            ProductScope = entity.ProductScope,
            JourneyScope = entity.JourneyScope,
            AudienceScope = entity.AudienceScope,
            ContextKey = entity.ContextKey,
            OriginUrl = entity.OriginUrl,
            OriginReference = entity.OriginReference,
            ThreadSummary = entity.ThreadSummary,
            ConfidenceScore = entity.ConfidenceScore,
            RevisionNumber = entity.RevisionNumber,
            AcceptedAnswerId = acceptedAnswer?.Id,
            DuplicateOfQuestionId = entity.DuplicateOfQuestionId,
            AnsweredAtUtc = entity.AnsweredAtUtc,
            ResolvedAtUtc = entity.ResolvedAtUtc,
            ValidatedAtUtc = entity.ValidatedAtUtc,
            LastActivityAtUtc = entity.LastActivityAtUtc,
            FeedbackScore = ThreadActivitySignals.ComputeFeedbackScore(entity.Activities),
            AcceptedAnswer = request.IncludeAnswers ? acceptedAnswer : null,
            Answers = request.IncludeAnswers ? publicAnswers : [],
            Topics = request.IncludeTopics
                ? entity.Topics.Select(link => link.Topic.ToTopicDto()).ToList()
                : [],
            Sources = request.IncludeSources ? publicSources : [],
            Activity = request.IncludeActivity
                ? entity.Activities
                    .OrderByDescending(activity => activity.OccurredAtUtc)
                    .Select(activity => activity.ToThreadActivityDto())
                    .ToList()
                : []
        };
    }

    public static AnswerDto ToPortalAnswerDto(
        this Answer entity,
        IEnumerable<ThreadActivity> questionActivity,
        Guid? acceptedAnswerId)
    {
        return ToAnswerDto(entity, questionActivity, acceptedAnswerId, false);
    }

    public static AnswerDto ToPublicAnswerDto(
        this Answer entity,
        IEnumerable<ThreadActivity> questionActivity,
        Guid? acceptedAnswerId)
    {
        return ToAnswerDto(entity, questionActivity, acceptedAnswerId, true);
    }

    public static QuestionSpaceDto ToQuestionSpaceDto(this QuestionSpace entity)
    {
        return new QuestionSpaceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Key = entity.Key,
            Summary = entity.Summary,
            DefaultLanguage = entity.DefaultLanguage,
            Kind = entity.Kind,
            Visibility = entity.Visibility,
            ModerationPolicy = entity.ModerationPolicy,
            SearchMarkupMode = entity.SearchMarkupMode,
            ProductScope = entity.ProductScope,
            JourneyScope = entity.JourneyScope,
            AcceptsQuestions = entity.AcceptsQuestions,
            AcceptsAnswers = entity.AcceptsAnswers,
            RequiresQuestionReview = entity.RequiresQuestionReview,
            RequiresAnswerReview = entity.RequiresAnswerReview,
            PublishedAtUtc = entity.PublishedAtUtc,
            LastValidatedAtUtc = entity.LastValidatedAtUtc,
            QuestionCount = entity.Questions.Count
        };
    }

    public static TopicDto ToTopicDto(this Topic entity)
    {
        return new TopicDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Category = entity.Category,
            Description = entity.Description
        };
    }

    public static KnowledgeSourceDto ToKnowledgeSourceDto(this KnowledgeSource entity)
    {
        return new KnowledgeSourceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Kind = entity.Kind,
            Locator = entity.Locator,
            Label = entity.Label,
            Scope = entity.Scope,
            SystemName = entity.SystemName,
            ExternalId = entity.ExternalId,
            Language = entity.Language,
            MediaType = entity.MediaType,
            Checksum = entity.Checksum,
            MetadataJson = entity.MetadataJson,
            Visibility = entity.Visibility,
            AllowsPublicCitation = entity.AllowsPublicCitation,
            AllowsPublicExcerpt = entity.AllowsPublicExcerpt,
            IsAuthoritative = entity.IsAuthoritative,
            CapturedAtUtc = entity.CapturedAtUtc,
            LastVerifiedAtUtc = entity.LastVerifiedAtUtc
        };
    }

    public static QuestionSourceLinkDto ToQuestionSourceLinkDto(this QuestionSourceLink entity)
    {
        return new QuestionSourceLinkDto
        {
            Id = entity.Id,
            QuestionId = entity.QuestionId,
            SourceId = entity.SourceId,
            Role = entity.Role,
            Order = entity.Order,
            Source = entity.Source?.ToKnowledgeSourceDto()
        };
    }

    public static AnswerSourceLinkDto ToAnswerSourceLinkDto(this AnswerSourceLink entity)
    {
        return new AnswerSourceLinkDto
        {
            Id = entity.Id,
            AnswerId = entity.AnswerId,
            SourceId = entity.SourceId,
            Role = entity.Role,
            Order = entity.Order,
            Source = entity.Source?.ToKnowledgeSourceDto()
        };
    }

    public static ThreadActivityDto ToThreadActivityDto(this ThreadActivity entity)
    {
        return new ThreadActivityDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            QuestionId = entity.QuestionId,
            AnswerId = entity.AnswerId,
            Kind = entity.Kind,
            ActorKind = entity.ActorKind,
            ActorLabel = entity.ActorLabel,
            UserPrint = entity.UserPrint,
            Notes = entity.Notes,
            MetadataJson = entity.MetadataJson,
            OccurredAtUtc = entity.OccurredAtUtc
        };
    }

    private static AnswerDto ToAnswerDto(
        Answer entity,
        IEnumerable<ThreadActivity> questionActivity,
        Guid? acceptedAnswerId,
        bool publicOnly)
    {
        IEnumerable<AnswerSourceLink> sources = entity.Sources;
        if (publicOnly)
            sources = sources.Where(link =>
                link.Source is not null &&
                link.Source.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed);

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
            Language = entity.Language,
            ContextKey = entity.ContextKey,
            ApplicabilityRulesJson = entity.ApplicabilityRulesJson,
            TrustNote = entity.TrustNote,
            EvidenceSummary = entity.EvidenceSummary,
            AuthorLabel = entity.AuthorLabel,
            ConfidenceScore = entity.ConfidenceScore,
            Rank = entity.Rank,
            RevisionNumber = entity.RevisionNumber,
            IsAccepted = entity.Id == acceptedAnswerId,
            IsOfficial = entity.Kind == AnswerKind.Official,
            PublishedAtUtc = entity.PublishedAtUtc,
            ValidatedAtUtc = entity.ValidatedAtUtc,
            AcceptedAtUtc = entity.AcceptedAtUtc,
            RetiredAtUtc = entity.RetiredAtUtc,
            VoteScore = ThreadActivitySignals.ComputeVoteScore(questionActivity, entity.Id),
            Sources = sources
                .OrderBy(source => source.Order)
                .Select(source => source.ToAnswerSourceLinkDto())
                .ToList()
        };
    }
}
