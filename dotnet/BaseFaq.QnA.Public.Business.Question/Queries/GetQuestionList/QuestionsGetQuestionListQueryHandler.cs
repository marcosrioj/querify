using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Question.Queries.GetQuestionList;

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
            .Include(question => question.Space)
            .Include(question => question.Activity)
            .Where(question =>
                question.TenantId == tenantId &&
                (question.Visibility == VisibilityScope.Public ||
                 question.Visibility == VisibilityScope.PublicIndexed) &&
                (question.Status == QuestionStatus.Open || question.Status == QuestionStatus.Answered ||
                 question.Status == QuestionStatus.Validated));

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(question =>
                EF.Functions.ILike(question.Title, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(question.Summary ?? string.Empty, $"%{request.Request.SearchText}%"));

        if (request.Request.SpaceId is not null)
            query = query.Where(question => question.SpaceId == request.Request.SpaceId);

        if (!string.IsNullOrWhiteSpace(request.Request.SpaceKey))
            query = query.Where(question => question.Space.Key == request.Request.SpaceKey);

        if (!string.IsNullOrWhiteSpace(request.Request.ContextKey))
            query = query.Where(question => question.ContextKey == request.Request.ContextKey);

        if (!string.IsNullOrWhiteSpace(request.Request.Language))
            query = query.Where(question => question.Language == request.Request.Language);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "title" => query.OrderBy(question => question.Title),
            "title desc" => query.OrderByDescending(question => question.Title),
            _ => query.OrderByDescending(question =>
                question.ResolvedAtUtc ?? question.AnsweredAtUtc ?? question.LastActivityAtUtc)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QuestionDto>(totalCount, items.Select(entity => entity.ToQuestionDto()).ToList());
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}