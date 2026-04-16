using System.Net;
using System.Text.Json;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AnswerEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Answer;
using AnswerSourceLinkEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.AnswerSourceLink;
using KnowledgeSourceEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.KnowledgeSource;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;
using QuestionSourceLinkEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.QuestionSourceLink;
using ThreadActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.ThreadActivity;
using TopicEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Topic;

namespace BaseFaq.QnA.Public.Business.Question.Queries;

public sealed class QuestionsGetQuestionByKeyQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsGetQuestionByKeyQuery, QuestionDetailDto>
{
    public async Task<QuestionDetailDto> Handle(QuestionsGetQuestionByKeyQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await BuildVisibleQuestionDetailQuery(tenantId)
            .AsNoTracking()
            .SingleOrDefaultAsync(question => question.Key == request.Key, cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Question '{request.Key}' was not found.", errorCode: (int)HttpStatusCode.NotFound)
            : MapQuestionDetail(entity, request.Request);
    }

    private IQueryable<QuestionEntity> BuildVisibleQuestionDetailQuery(Guid tenantId)
    {
        return dbContext.Questions
            .Include(question => question.Space)
            .Include(question => question.AcceptedAnswer)
            .ThenInclude(answer => answer!.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.Answers)
            .ThenInclude(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.QuestionTopics)
            .ThenInclude(link => link.Topic)
            .Include(question => question.Activity)
            .Where(question =>
                question.TenantId == tenantId &&
                (question.Visibility == VisibilityScope.Public || question.Visibility == VisibilityScope.PublicIndexed) &&
                (question.Status == QuestionStatus.Open || question.Status == QuestionStatus.Answered || question.Status == QuestionStatus.Validated));
    }

    private static QuestionDetailDto MapQuestionDetail(QuestionEntity entity, QuestionGetRequestDto request)
    {
        var publicSources = entity.Sources
            .Where(link => link.Source.Visibility == VisibilityScope.Public || link.Source.Visibility == VisibilityScope.PublicIndexed)
            .Select(MapQuestionSourceLink)
            .ToList();
        var publicAnswers = entity.Answers
            .Where(answer =>
                (answer.Visibility == VisibilityScope.Public || answer.Visibility == VisibilityScope.PublicIndexed) &&
                (answer.Status == AnswerStatus.Published || answer.Status == AnswerStatus.Validated))
            .OrderByDescending(answer => answer.IsAccepted)
            .ThenByDescending(answer => answer.Rank)
            .Select(answer => MapAnswer(answer, entity.Activity))
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
            FeedbackScore = ComputeFeedbackScore(entity.Activity),
            AcceptedAnswer = request.IncludeAnswers ? acceptedAnswer : null,
            Answers = request.IncludeAnswers ? publicAnswers : [],
            Topics = request.IncludeTopics ? entity.QuestionTopics.Select(link => MapTopic(link.Topic)).ToList() : [],
            Sources = request.IncludeSources ? publicSources : [],
            Activity = request.IncludeActivity
                ? entity.Activity.OrderByDescending(activity => activity.OccurredAtUtc).Select(MapThreadActivity).ToList()
                : []
        };
    }

    private static AnswerDto MapAnswer(AnswerEntity entity, IEnumerable<ThreadActivityEntity> questionActivity)
    {
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
            IsAccepted = entity.IsAccepted,
            IsCanonical = entity.IsCanonical,
            IsOfficial = entity.IsOfficial,
            PublishedAtUtc = entity.PublishedAtUtc,
            ValidatedAtUtc = entity.ValidatedAtUtc,
            AcceptedAtUtc = entity.AcceptedAtUtc,
            RetiredAtUtc = entity.RetiredAtUtc,
            VoteScore = ComputeVoteScore(questionActivity, entity.Id),
            Sources = entity.Sources
                .Where(link => link.Source.Visibility == VisibilityScope.Public || link.Source.Visibility == VisibilityScope.PublicIndexed)
                .Select(MapAnswerSourceLink)
                .ToList()
        };
    }

    private static QuestionSourceLinkDto MapQuestionSourceLink(QuestionSourceLinkEntity entity)
    {
        return new QuestionSourceLinkDto
        {
            Id = entity.Id,
            QuestionId = entity.QuestionId,
            SourceId = entity.SourceId,
            Role = entity.Role,
            Label = entity.Label,
            Scope = entity.Scope,
            Excerpt = entity.Excerpt,
            Order = entity.Order,
            ConfidenceScore = entity.ConfidenceScore,
            IsPrimary = entity.IsPrimary,
            Source = entity.Source is null ? null : MapKnowledgeSource(entity.Source)
        };
    }

    private static AnswerSourceLinkDto MapAnswerSourceLink(AnswerSourceLinkEntity entity)
    {
        return new AnswerSourceLinkDto
        {
            Id = entity.Id,
            AnswerId = entity.AnswerId,
            SourceId = entity.SourceId,
            Role = entity.Role,
            Label = entity.Label,
            Scope = entity.Scope,
            Excerpt = entity.Excerpt,
            Order = entity.Order,
            ConfidenceScore = entity.ConfidenceScore,
            IsPrimary = entity.IsPrimary,
            Source = entity.Source is null ? null : MapKnowledgeSource(entity.Source)
        };
    }

    private static KnowledgeSourceDto MapKnowledgeSource(KnowledgeSourceEntity entity)
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

    private static TopicDto MapTopic(TopicEntity entity)
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

    private static ThreadActivityDto MapThreadActivity(ThreadActivityEntity entity)
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
            Notes = entity.Notes,
            MetadataJson = entity.MetadataJson,
            SnapshotJson = entity.SnapshotJson,
            RevisionNumber = entity.RevisionNumber,
            OccurredAtUtc = entity.OccurredAtUtc
        };
    }

    private static int ComputeFeedbackScore(IEnumerable<ThreadActivityEntity> activities)
    {
        return activities
            .Where(activity => activity.Kind == ActivityKind.FeedbackReceived)
            .Select(activity => new { activity, metadata = ParseFeedback(activity.MetadataJson) })
            .Where(item => item.metadata is not null)
            .GroupBy(item => item.metadata!.UserPrint)
            .Select(group => group.OrderByDescending(item => item.activity.OccurredAtUtc).First().metadata!)
            .Sum(metadata => metadata.Like ? 1 : -1);
    }

    private static int ComputeVoteScore(IEnumerable<ThreadActivityEntity> activities, Guid answerId)
    {
        return activities
            .Where(activity => activity.Kind == ActivityKind.VoteReceived && activity.AnswerId == answerId)
            .Select(activity => new { activity, metadata = ParseVote(activity.MetadataJson) })
            .Where(item => item.metadata is not null)
            .GroupBy(item => item.metadata!.UserPrint)
            .Select(group => group.OrderByDescending(item => item.activity.OccurredAtUtc).First().metadata!)
            .Sum(metadata => metadata.VoteValue);
    }

    private static FeedbackMetadata? ParseFeedback(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<FeedbackMetadata>(metadataJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static VoteMetadata? ParseVote(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<VoteMetadata>(metadataJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class FeedbackMetadata
    {
        public required string UserPrint { get; init; }
        public required bool Like { get; init; }
    }

    private sealed class VoteMetadata
    {
        public required string UserPrint { get; init; }
        public required int VoteValue { get; init; }
    }
}
