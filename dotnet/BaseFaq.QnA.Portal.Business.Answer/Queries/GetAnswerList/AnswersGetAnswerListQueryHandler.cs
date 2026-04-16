using BaseFaq.Models.Common.Dtos;
using System.Text.Json;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AnswerEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Answer;
using AnswerSourceLinkEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.AnswerSourceLink;
using KnowledgeSourceEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.KnowledgeSource;
using ThreadActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.ThreadActivity;

namespace BaseFaq.QnA.Portal.Business.Answer.Queries;

public sealed class AnswersGetAnswerListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersGetAnswerListQuery, PagedResultDto<AnswerDto>>
{
    public async Task<PagedResultDto<AnswerDto>> Handle(AnswersGetAnswerListQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        IQueryable<AnswerEntity> query = dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activity)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .Where(answer => answer.TenantId == tenantId);

        if (request.Request.QuestionId is not null)
        {
            query = query.Where(answer => answer.QuestionId == request.Request.QuestionId);
        }

        if (request.Request.Status is not null)
        {
            query = query.Where(answer => answer.Status == request.Request.Status);
        }

        if (request.Request.Visibility is not null)
        {
            query = query.Where(answer => answer.Visibility == request.Request.Visibility);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.ContextKey))
        {
            query = query.Where(answer => answer.ContextKey == request.Request.ContextKey);
        }

        if (request.Request.IsAccepted is not null)
        {
            query = query.Where(answer => answer.IsAccepted == request.Request.IsAccepted);
        }

        if (request.Request.IsCanonical is not null)
        {
            query = query.Where(answer => answer.IsCanonical == request.Request.IsCanonical);
        }

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "headline desc" => query.OrderByDescending(answer => answer.Headline),
            "rank" => query.OrderBy(answer => answer.Rank),
            "rank desc" => query.OrderByDescending(answer => answer.Rank),
            _ => query.OrderByDescending(answer => answer.IsAccepted).ThenByDescending(answer => answer.Rank)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AnswerDto>(totalCount, items.Select(MapAnswer).ToList());
    }

    private static AnswerDto MapAnswer(AnswerEntity entity)
    {
        var activity = entity.Question?.Activity?.ToList() ?? [];

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
            VoteScore = ComputeVoteScore(activity, entity.Id),
            Sources = entity.Sources
                .OrderBy(source => source.Order)
                .Select(MapAnswerSourceLink)
                .ToList()
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

    private sealed class VoteMetadata
    {
        public required string UserPrint { get; init; }
        public required int VoteValue { get; init; }
    }
}
