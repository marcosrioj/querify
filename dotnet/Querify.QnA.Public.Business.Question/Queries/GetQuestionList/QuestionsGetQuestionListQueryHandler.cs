using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Constants;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Public.Business.Question.Queries.GetQuestionList;

public sealed class QuestionsGetQuestionListQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsGetQuestionListQuery, PagedResultDto<QuestionDto>>
{
    public async Task<PagedResultDto<QuestionDto>> Handle(QuestionsGetQuestionListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var query = dbContext.Questions
            .AsNoTracking()
            .Where(question =>
                question.TenantId == tenantId &&
                question.Visibility == VisibilityScope.Public &&
                question.Status == QuestionStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(question =>
                EF.Functions.ILike(question.Title, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(question.Summary ?? string.Empty, $"%{request.Request.SearchText}%"));

        if (request.Request.SpaceId is not null)
            query = query.Where(question => question.SpaceId == request.Request.SpaceId);

        if (!string.IsNullOrWhiteSpace(request.Request.SpaceSlug))
            query = query.Where(question => question.Space.Slug == request.Request.SpaceSlug);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "sort" => query.OrderBy(question => question.Sort),
            "sort desc" => query.OrderByDescending(question => question.Sort),
            "title" => query.OrderBy(question => question.Title),
            "title desc" => query.OrderByDescending(question => question.Title),
            _ => query.OrderByDescending(question => question.LastActivityAtUtc)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(question => new QuestionDto
            {
                Id = question.Id,
                TenantId = question.TenantId,
                SpaceId = question.SpaceId,
                SpaceSlug = question.Space.Slug,
                Title = question.Title,
                Summary = question.Summary,
                ContextNote = question.ContextNote,
                Status = question.Status,
                Visibility = question.Visibility,
                OriginChannel = question.OriginChannel,
                AiConfidenceScore = question.AiConfidenceScore,
                FeedbackScore = question.FeedbackScore,
                Sort = question.Sort,
                AcceptedAnswerId = question.AcceptedAnswerId,
                LastActivityAtUtc = question.LastActivityAtUtc,
                LastUpdatedAtUtc = question.UpdatedDate ?? question.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QuestionDto>(totalCount, items);
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}
