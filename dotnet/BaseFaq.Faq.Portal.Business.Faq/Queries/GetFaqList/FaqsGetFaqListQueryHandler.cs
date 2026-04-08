using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Faq;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqList;

public class FaqsGetFaqListQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqsGetFaqListQuery, PagedResultDto<FaqDto>>
{
    public async Task<PagedResultDto<FaqDto>> Handle(FaqsGetFaqListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.Faqs.AsNoTracking();
        query = ApplyFilters(query, request.Request);
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(faq => new FaqDto
            {
                Id = faq.Id,
                Name = faq.Name,
                Language = faq.Language,
                Status = faq.Status
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<FaqDto>(totalCount, items);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.Faq> ApplyFilters(
        IQueryable<Common.Persistence.FaqDb.Entities.Faq> query,
        FaqGetAllRequestDto request)
    {
        if (request.FaqIds is { Count: > 0 })
        {
            query = query.Where(faq => request.FaqIds.Contains(faq.Id));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(faq => faq.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var pattern = $"%{request.SearchText.Trim()}%";
            query = query.Where(faq =>
                EF.Functions.ILike(faq.Name, pattern) ||
                EF.Functions.ILike(faq.Language, pattern));
        }

        return query;
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
