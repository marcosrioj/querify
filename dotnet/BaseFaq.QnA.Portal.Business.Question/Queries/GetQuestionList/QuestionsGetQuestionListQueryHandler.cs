using BaseFaq.Models.Common.Dtos;
using System.Text.Json;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;
using ThreadActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.ThreadActivity;

namespace BaseFaq.QnA.Portal.Business.Question.Queries;

public sealed class QuestionsGetQuestionListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsGetQuestionListQuery, PagedResultDto<QuestionDto>>
{
    public async Task<PagedResultDto<QuestionDto>> Handle(QuestionsGetQuestionListQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        IQueryable<QuestionEntity> query = dbContext.Questions
            .Include(question => question.Space)
            .Include(question => question.Activity)
            .Where(question => question.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
        {
            query = query.Where(question =>
                EF.Functions.ILike(question.Title, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(question.Key, $"%{request.Request.SearchText}%"));
        }

        if (request.Request.SpaceId is not null)
        {
            query = query.Where(question => question.SpaceId == request.Request.SpaceId);
        }

        if (request.Request.AcceptedAnswerId is not null)
        {
            query = query.Where(question => question.AcceptedAnswerId == request.Request.AcceptedAnswerId);
        }

        if (request.Request.DuplicateOfQuestionId is not null)
        {
            query = query.Where(question => question.DuplicateOfQuestionId == request.Request.DuplicateOfQuestionId);
        }

        if (request.Request.Status is not null)
        {
            query = query.Where(question => question.Status == request.Request.Status);
        }

        if (request.Request.Visibility is not null)
        {
            query = query.Where(question => question.Visibility == request.Request.Visibility);
        }

        if (request.Request.Kind is not null)
        {
            query = query.Where(question => question.Kind == request.Request.Kind);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.SpaceKey))
        {
            query = query.Where(question => question.Space.Key == request.Request.SpaceKey);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.ContextKey))
        {
            query = query.Where(question => question.ContextKey == request.Request.ContextKey);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Language))
        {
            query = query.Where(question => question.Language == request.Request.Language);
        }

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "title desc" => query.OrderByDescending(question => question.Title),
            "resolvedatutc desc" => query.OrderByDescending(question => question.ResolvedAtUtc),
            "resolvedatutc" => query.OrderBy(question => question.ResolvedAtUtc),
            _ => query.OrderByDescending(question => question.LastActivityAtUtc ?? question.UpdatedDate ?? question.CreatedDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QuestionDto>(totalCount, items.Select(MapQuestion).ToList());
    }

    private static QuestionDto MapQuestion(QuestionEntity entity)
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
            FeedbackScore = ComputeFeedbackScore(entity.Activity)
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

    private sealed class FeedbackMetadata
    {
        public required string UserPrint { get; init; }
        public required bool Like { get; init; }
    }
}
