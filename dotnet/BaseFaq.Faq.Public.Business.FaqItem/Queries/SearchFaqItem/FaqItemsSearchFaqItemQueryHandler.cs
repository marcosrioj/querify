using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Projections;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Public.Business.FaqItem.Queries.SearchFaqItem;

public class FaqItemsSearchFaqItemQueryHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FaqItemsSearchFaqItemQuery, PagedResultDto<FaqItemDto>>
{
    public async Task<PagedResultDto<FaqItemDto>> Handle(
        FaqItemsSearchFaqItemQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var query = BuildSearchQuery(request, tenantId);
        var totalCount = await query.CountAsync(cancellationToken);
        var groupByFaq = request.Request.FaqIds is { Count: > 1 };
        query = ApplyDefaultSort(query, groupByFaq);
        var items = await LoadItemsAsync(query, request, cancellationToken);

        return new PagedResultDto<FaqItemDto>(totalCount, items);
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> BuildSearchQuery(
        FaqItemsSearchFaqItemQuery request,
        Guid tenantId)
    {
        var query = dbContext.FaqItems
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId);

        if (request.Request.FaqIds is { Count: > 0 })
        {
            query = query.Where(item => request.Request.FaqIds!.Contains(item.FaqId));
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(item =>
                item.Question.Contains(term) ||
                item.Answers.Any(answer => answer.ShortAnswer.Contains(term)) ||
                item.Answers.Any(answer => answer.Answer != null && answer.Answer.Contains(term)) ||
                (item.AdditionalInfo != null && item.AdditionalInfo.Contains(term)));
        }

        return query;
    }

    private static async Task<List<FaqItemDto>> LoadItemsAsync(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> query,
        FaqItemsSearchFaqItemQuery request,
        CancellationToken cancellationToken)
    {
        return await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .SelectPublicFaqItemDtos()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> ApplyDefaultSort(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> query,
        bool groupByFaq)
    {
        if (groupByFaq)
        {
            return query
                .OrderBy(item => item.FaqId)
                .ThenBy(item => item.Sort)
                .ThenByDescending(item => item.FeedbackScore)
                .ThenByDescending(item => item.ConfidenceScore)
                .ThenByDescending(item => item.UpdatedDate ?? DateTime.MinValue);
        }

        return query
            .OrderBy(item => item.Sort)
            .ThenByDescending(item => item.FeedbackScore)
            .ThenByDescending(item => item.ConfidenceScore)
            .ThenByDescending(item => item.UpdatedDate ?? DateTime.MinValue);
    }
}
