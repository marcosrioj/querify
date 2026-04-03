using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.ContentRef;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.ContentRef.Queries.GetContentRefList;

public class ContentRefsGetContentRefListQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<ContentRefsGetContentRefListQuery, PagedResultDto<ContentRefDto>>
{
    public async Task<PagedResultDto<ContentRefDto>> Handle(
        ContentRefsGetContentRefListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.ContentRefs.AsNoTracking();
        query = ApplyFilters(query, request.Request, dbContext);
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(contentRef => new ContentRefDto
            {
                Id = contentRef.Id,
                Kind = contentRef.Kind,
                Locator = contentRef.Locator,
                Label = contentRef.Label,
                Scope = contentRef.Scope
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ContentRefDto>(totalCount, items);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.ContentRef> ApplyFilters(
        IQueryable<Common.Persistence.FaqDb.Entities.ContentRef> query,
        ContentRefGetAllRequestDto request,
        FaqDbContext dbContext)
    {
        if (request.Kind.HasValue)
        {
            query = query.Where(contentRef => contentRef.Kind == request.Kind.Value);
        }

        if (request.FaqId.HasValue)
        {
            query = query.Where(contentRef =>
                dbContext.FaqContentRefs.Any(faqContentRef =>
                    faqContentRef.FaqId == request.FaqId.Value &&
                    faqContentRef.ContentRefId == contentRef.Id) ||
                dbContext.FaqItems.Any(faqItem =>
                    faqItem.FaqId == request.FaqId.Value &&
                    faqItem.ContentRefId == contentRef.Id));
        }

        if (request.FaqItemId.HasValue)
        {
            query = query.Where(contentRef =>
                dbContext.FaqItems.Any(faqItem =>
                    faqItem.Id == request.FaqItemId.Value &&
                    faqItem.ContentRefId == contentRef.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var pattern = $"%{request.SearchText.Trim()}%";
            query = query.Where(contentRef =>
                EF.Functions.ILike(contentRef.Locator, pattern) ||
                (contentRef.Label != null && EF.Functions.ILike(contentRef.Label, pattern)) ||
                (contentRef.Scope != null && EF.Functions.ILike(contentRef.Scope, pattern)));
        }

        return query;
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.ContentRef> ApplySorting(
        IQueryable<Common.Persistence.FaqDb.Entities.ContentRef> query,
        string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(contentRef => contentRef.UpdatedDate);
        }

        IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>? orderedQuery = null;
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

        return orderedQuery ?? query.OrderByDescending(contentRef => contentRef.UpdatedDate);
    }

    private static IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef> ApplyOrder(
        IQueryable<Common.Persistence.FaqDb.Entities.ContentRef> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "kind" => isFirst
                ? (desc
                    ? query.OrderByDescending(contentRef => contentRef.Kind)
                    : query.OrderBy(contentRef => contentRef.Kind))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenByDescending(contentRef => contentRef.Kind)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenBy(contentRef => contentRef.Kind)),
            "locator" => isFirst
                ? (desc
                    ? query.OrderByDescending(contentRef => contentRef.Locator)
                    : query.OrderBy(contentRef => contentRef.Locator))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenByDescending(contentRef => contentRef.Locator)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenBy(contentRef => contentRef.Locator)),
            "label" => isFirst
                ? (desc
                    ? query.OrderByDescending(contentRef => contentRef.Label)
                    : query.OrderBy(contentRef => contentRef.Label))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenByDescending(contentRef => contentRef.Label)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenBy(contentRef => contentRef.Label)),
            "scope" => isFirst
                ? (desc
                    ? query.OrderByDescending(contentRef => contentRef.Scope)
                    : query.OrderBy(contentRef => contentRef.Scope))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenByDescending(contentRef => contentRef.Scope)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenBy(contentRef => contentRef.Scope)),
            "createddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(contentRef => contentRef.CreatedDate)
                    : query.OrderBy(contentRef => contentRef.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenByDescending(contentRef => contentRef.CreatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenBy(contentRef => contentRef.CreatedDate)),
            "updateddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(contentRef => contentRef.UpdatedDate)
                    : query.OrderBy(contentRef => contentRef.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenByDescending(contentRef => contentRef.UpdatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenBy(contentRef => contentRef.UpdatedDate)),
            "id" => isFirst
                ? (desc
                    ? query.OrderByDescending(contentRef => contentRef.Id)
                    : query.OrderBy(contentRef => contentRef.Id))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenByDescending(contentRef => contentRef.Id)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                    .ThenBy(contentRef => contentRef.Id)),
            _ => isFirst
                ? query.OrderByDescending(contentRef => contentRef.UpdatedDate)
                : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.ContentRef>)query)
                .ThenByDescending(contentRef => contentRef.UpdatedDate)
        };
    }
}
