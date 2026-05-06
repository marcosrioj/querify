using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Dtos;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Answer.Queries.GetAnswerList;

public sealed class AnswersGetAnswerListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersGetAnswerListQuery, PagedResultDto<AnswerDto>>
{
    public async Task<PagedResultDto<AnswerDto>> Handle(AnswersGetAnswerListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var query = dbContext.Answers
            .AsNoTracking()
            .Where(answer => answer.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(answer =>
                EF.Functions.ILike(answer.Headline, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.Body ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.ContextNote ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.AuthorLabel ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.Question.Title, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.Question.Space.Slug, $"%{request.Request.SearchText}%"));

        if (request.Request.SpaceId is not null)
            query = query.Where(answer => answer.Question.SpaceId == request.Request.SpaceId);

        if (request.Request.SourceId is not null)
            query = query.Where(answer => answer.Sources.Any(link => link.SourceId == request.Request.SourceId.Value));

        if (request.Request.QuestionId is not null)
            query = query.Where(answer => answer.QuestionId == request.Request.QuestionId);

        if (request.Request.Status is not null) query = query.Where(answer => answer.Status == request.Request.Status);

        if (request.Request.Visibility is not null)
            query = query.Where(answer => answer.Visibility == request.Request.Visibility);

        if (request.Request.IsAccepted is not null)
            query = request.Request.IsAccepted.Value
                ? query.Where(answer => answer.Question != null && answer.Question.AcceptedAnswerId == answer.Id)
                : query.Where(answer => answer.Question == null || answer.Question.AcceptedAnswerId != answer.Id);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "lastupdatedatutc" or "lastupdatedatutc desc" or "updateddate" or "updateddate desc" =>
                query.OrderByDescending(answer => answer.UpdatedDate ?? answer.CreatedDate),
            "lastupdatedatutc asc" or "updateddate asc" => query.OrderBy(answer =>
                answer.UpdatedDate ?? answer.CreatedDate),
            "headline" or "headline asc" => query.OrderBy(answer => answer.Headline),
            "headline desc" => query.OrderByDescending(answer => answer.Headline),
            "score" or "score asc" => query.OrderBy(answer => answer.Score),
            "score desc" => query.OrderByDescending(answer => answer.Score),
            "sort" or "sort asc" => query.OrderBy(answer => answer.Sort),
            "sort desc" => query.OrderByDescending(answer => answer.Sort),
            "aiconfidencescore" or "aiconfidencescore asc" => query.OrderBy(answer => answer.AiConfidenceScore),
            "aiconfidencescore desc" => query.OrderByDescending(answer => answer.AiConfidenceScore),
            "status" or "status asc" => query.OrderBy(answer => answer.Status),
            "status desc" => query.OrderByDescending(answer => answer.Status),
            "kind" or "kind asc" => query.OrderBy(answer => answer.Kind),
            "kind desc" => query.OrderByDescending(answer => answer.Kind),
            _ => query.OrderByDescending(answer => answer.UpdatedDate ?? answer.CreatedDate)
                .ThenByDescending(answer => answer.Question != null && answer.Question.AcceptedAnswerId == answer.Id)
                .ThenBy(answer => answer.Headline)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(answer => new AnswerDto
            {
                Id = answer.Id,
                TenantId = answer.TenantId,
                QuestionId = answer.QuestionId,
                Headline = answer.Headline,
                Body = answer.Body,
                Kind = answer.Kind,
                Status = answer.Status,
                Visibility = answer.Visibility,
                ContextNote = answer.ContextNote,
                AuthorLabel = answer.AuthorLabel,
                AiConfidenceScore = answer.AiConfidenceScore,
                Score = answer.Score,
                Sort = answer.Sort,
                IsAccepted = answer.Question.AcceptedAnswerId == answer.Id,
                IsOfficial = answer.Kind == AnswerKind.Official,
                LastUpdatedAtUtc = answer.UpdatedDate ?? answer.CreatedDate,
                VoteScore = 0,
                Sources = answer.Sources
                    .OrderBy(source => source.Order)
                    .Select(source => new AnswerSourceLinkDto
                    {
                        Id = source.Id,
                        AnswerId = source.AnswerId,
                        SourceId = source.SourceId,
                        Role = source.Role,
                        Order = source.Order,
                        Source = new SourceDto
                        {
                            Id = source.Source.Id,
                            TenantId = source.Source.TenantId,
                            Kind = source.Source.Kind,
                            Locator = source.Source.Locator,
                            Label = source.Source.Label,
                            ContextNote = source.Source.ContextNote,
                            ExternalId = source.Source.ExternalId,
                            Language = source.Source.Language,
                            MediaType = source.Source.MediaType,
                            Checksum = source.Source.Checksum,
                            MetadataJson = source.Source.MetadataJson,
                            Visibility = source.Source.Visibility,
                            LastVerifiedAtUtc = source.Source.LastVerifiedAtUtc,
                            LastUpdatedAtUtc = source.Source.UpdatedDate ?? source.Source.CreatedDate,
                            SpaceUsageCount = source.Source.Spaces.Count,
                            QuestionUsageCount = source.Source.Questions.Count,
                            AnswerUsageCount = source.Source.Answers.Count
                        }
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        await PopulateVoteScoresAsync(tenantId, items, cancellationToken);

        return new PagedResultDto<AnswerDto>(
            totalCount,
            items);
    }

    private async Task PopulateVoteScoresAsync(
        Guid tenantId,
        IReadOnlyCollection<AnswerDto> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return;

        var questionIds = items.Select(answer => answer.QuestionId).Distinct().ToList();
        var answerIds = items.Select(answer => answer.Id).ToHashSet();

        var voteSignals = await dbContext.Activities
            .AsNoTracking()
            .Where(activity =>
                activity.TenantId == tenantId &&
                activity.Kind == ActivityKind.VoteReceived &&
                activity.AnswerId.HasValue &&
                questionIds.Contains(activity.QuestionId) &&
                answerIds.Contains(activity.AnswerId.Value))
            .Select(activity => new ActivitySignalEntry(
                activity.Kind,
                activity.AnswerId,
                activity.OccurredAtUtc,
                activity.UserPrint,
                activity.MetadataJson))
            .ToListAsync(cancellationToken);

        foreach (var item in items)
            item.VoteScore = ActivitySignals.ComputeVoteScore(voteSignals, item.Id);
    }
}
