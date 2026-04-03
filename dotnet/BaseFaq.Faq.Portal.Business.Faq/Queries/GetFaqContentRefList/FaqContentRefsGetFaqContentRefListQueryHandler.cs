using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqContentRef;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqContentRefList;

public class FaqContentRefsGetFaqContentRefListQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqContentRefsGetFaqContentRefListQuery, PagedResultDto<FaqContentRefDto>>
{
    public async Task<PagedResultDto<FaqContentRefDto>> Handle(
        FaqContentRefsGetFaqContentRefListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.FaqContentRefs.AsNoTracking();
        query = ApplyFilters(query, request.Request);
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(faqContentRef => new FaqContentRefDto
            {
                Id = faqContentRef.Id,
                FaqId = faqContentRef.FaqId,
                ContentRefId = faqContentRef.ContentRefId
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<FaqContentRefDto>(totalCount, items);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef> ApplyFilters(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef> query,
        FaqContentRefGetAllRequestDto request)
    {
        if (request.FaqId.HasValue)
        {
            query = query.Where(faqContentRef => faqContentRef.FaqId == request.FaqId.Value);
        }

        if (request.ContentRefId.HasValue)
        {
            query = query.Where(faqContentRef => faqContentRef.ContentRefId == request.ContentRefId.Value);
        }

        return query;
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef> ApplySorting(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef> query,
        string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(faqContentRef => faqContentRef.UpdatedDate);
        }

        IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>? orderedQuery = null;
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

        return orderedQuery ?? query.OrderByDescending(faqContentRef => faqContentRef.UpdatedDate);
    }

    private static IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef> ApplyOrder(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "faqid" => isFirst
                ? (desc
                    ? query.OrderByDescending(faqContentRef => faqContentRef.FaqId)
                    : query.OrderBy(faqContentRef => faqContentRef.FaqId))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenByDescending(faqContentRef => faqContentRef.FaqId)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenBy(faqContentRef => faqContentRef.FaqId)),
            "contentrefid" => isFirst
                ? (desc
                    ? query.OrderByDescending(faqContentRef => faqContentRef.ContentRefId)
                    : query.OrderBy(faqContentRef => faqContentRef.ContentRefId))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenByDescending(faqContentRef => faqContentRef.ContentRefId)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenBy(faqContentRef => faqContentRef.ContentRefId)),
            "createddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(faqContentRef => faqContentRef.CreatedDate)
                    : query.OrderBy(faqContentRef => faqContentRef.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenByDescending(faqContentRef => faqContentRef.CreatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenBy(faqContentRef => faqContentRef.CreatedDate)),
            "updateddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(faqContentRef => faqContentRef.UpdatedDate)
                    : query.OrderBy(faqContentRef => faqContentRef.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenByDescending(faqContentRef => faqContentRef.UpdatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenBy(faqContentRef => faqContentRef.UpdatedDate)),
            "id" => isFirst
                ? (desc
                    ? query.OrderByDescending(faqContentRef => faqContentRef.Id)
                    : query.OrderBy(faqContentRef => faqContentRef.Id))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenByDescending(faqContentRef => faqContentRef.Id)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                    .ThenBy(faqContentRef => faqContentRef.Id)),
            _ => isFirst
                ? query.OrderByDescending(faqContentRef => faqContentRef.UpdatedDate)
                : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqContentRef>)query)
                .ThenByDescending(faqContentRef => faqContentRef.UpdatedDate)
        };
    }
}
