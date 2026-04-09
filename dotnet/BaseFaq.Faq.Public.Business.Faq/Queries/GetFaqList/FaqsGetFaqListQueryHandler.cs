using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.ContentRef;
using BaseFaq.Models.Faq.Dtos.Faq;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using BaseFaq.Models.Faq.Dtos.Tag;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Public.Business.Faq.Queries.GetFaqList;

public class FaqsGetFaqListQueryHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FaqsGetFaqListQuery, PagedResultDto<FaqDetailDto>>
{
    public async Task<PagedResultDto<FaqDetailDto>> Handle(FaqsGetFaqListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var includeItems = request.Request.IncludeFaqItems;
        var includeContentRefs = request.Request.IncludeContentRefs;
        var includeTags = request.Request.IncludeTags;

        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var query = BuildFaqQuery(request, tenantId);
        query = ApplyRequestedIncludes(query, includeItems, includeContentRefs, includeTags);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await LoadItemsAsync(query, request, includeItems, includeContentRefs, includeTags,
            cancellationToken);

        return new PagedResultDto<FaqDetailDto>(totalCount, items);
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private IQueryable<Common.Persistence.FaqDb.Entities.Faq> BuildFaqQuery(
        FaqsGetFaqListQuery request,
        Guid tenantId)
    {
        var query = dbContext.Faqs
            .AsNoTracking()
            .Where(faq => faq.TenantId == tenantId);

        if (request.Request.FaqIds is { Count: > 0 })
        {
            query = query.Where(faq => request.Request.FaqIds!.Contains(faq.Id));
        }

        return ApplySorting(query, request.Request.Sorting);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.Faq> ApplyRequestedIncludes(
        IQueryable<Common.Persistence.FaqDb.Entities.Faq> query,
        bool includeItems,
        bool includeContentRefs,
        bool includeTags)
    {
        if (includeItems)
        {
            query = query.Include(faq => faq.Items);
        }

        if (includeContentRefs)
        {
            query = query.Include(faq => faq.ContentRefs).ThenInclude(faqContentRef => faqContentRef.ContentRef);
        }

        if (includeTags)
        {
            query = query.Include(faq => faq.Tags).ThenInclude(faqTag => faqTag.Tag);
        }

        return query;
    }

    private static async Task<List<FaqDetailDto>> LoadItemsAsync(
        IQueryable<Common.Persistence.FaqDb.Entities.Faq> query,
        FaqsGetFaqListQuery request,
        bool includeItems,
        bool includeContentRefs,
        bool includeTags,
        CancellationToken cancellationToken)
    {
        return await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(faq => new FaqDetailDto
            {
                Id = faq.Id,
                Name = faq.Name,
                Language = faq.Language,
                Status = faq.Status,
                Items = includeItems
                    ? faq.Items.Select(item => new FaqItemDto
                    {
                        Id = item.Id,
                        Question = item.Question,
                        ShortAnswer = item.ShortAnswer,
                        Answer = item.Answer,
                        AdditionalInfo = item.AdditionalInfo,
                        CtaTitle = item.CtaTitle,
                        CtaUrl = item.CtaUrl,
                        Sort = item.Sort,
                        FeedbackScore = item.FeedbackScore,
                        AiConfidenceScore = item.AiConfidenceScore,
                        IsActive = item.IsActive,
                        FaqId = item.FaqId,
                        ContentRefId = item.ContentRefId
                    }).ToList()
                    : null,
                ContentRefs = includeContentRefs
                    ? faq.ContentRefs.Select(faqContentRef => new ContentRefDto
                    {
                        Id = faqContentRef.ContentRef.Id,
                        Kind = faqContentRef.ContentRef.Kind,
                        Locator = faqContentRef.ContentRef.Locator,
                        Label = faqContentRef.ContentRef.Label,
                        Scope = faqContentRef.ContentRef.Scope
                    }).ToList()
                    : null,
                Tags = includeTags
                    ? faq.Tags.Select(faqTag => new TagDto
                    {
                        Id = faqTag.Tag.Id,
                        Value = faqTag.Tag.Value
                    }).ToList()
                    : null
            })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.Faq> ApplySorting(
        IQueryable<Common.Persistence.FaqDb.Entities.Faq> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(faq => faq.UpdatedDate);
        }

        IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>? orderedQuery = null;
        var fields = sorting.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            var fieldName = parts[0];
            var desc = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

            orderedQuery = ApplyOrder(orderedQuery ?? query, fieldName, desc, orderedQuery is null);
        }

        return orderedQuery ?? query.OrderByDescending(faq => faq.UpdatedDate);
    }

    private static IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq> ApplyOrder(
        IQueryable<Common.Persistence.FaqDb.Entities.Faq> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "name" => isFirst
                ? (desc ? query.OrderByDescending(faq => faq.Name) : query.OrderBy(faq => faq.Name))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query)
                    .ThenByDescending(faq => faq.Name)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenBy(faq => faq.Name)),
            "language" => isFirst
                ? (desc ? query.OrderByDescending(faq => faq.Language) : query.OrderBy(faq => faq.Language))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenByDescending(faq =>
                        faq.Language)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenBy(faq => faq.Language)),
            "status" => isFirst
                ? (desc ? query.OrderByDescending(faq => faq.Status) : query.OrderBy(faq => faq.Status))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenByDescending(faq =>
                        faq.Status)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenBy(faq => faq.Status)),
            "createddate" => isFirst
                ? (desc ? query.OrderByDescending(faq => faq.CreatedDate) : query.OrderBy(faq => faq.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query)
                    .ThenByDescending(faq => faq.CreatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenBy(faq => faq.CreatedDate)),
            "updateddate" => isFirst
                ? (desc ? query.OrderByDescending(faq => faq.UpdatedDate) : query.OrderBy(faq => faq.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query)
                    .ThenByDescending(faq => faq.UpdatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenBy(faq => faq.UpdatedDate)),
            "id" => isFirst
                ? (desc ? query.OrderByDescending(faq => faq.Id) : query.OrderBy(faq => faq.Id))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenByDescending(faq => faq.Id)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query).ThenBy(faq => faq.Id)),
            _ => isFirst
                ? query.OrderByDescending(faq => faq.UpdatedDate)
                : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Faq>)query)
                .ThenByDescending(faq => faq.UpdatedDate)
        };
    }
}