using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Tenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantList;

public class TenantsGetTenantListQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantsGetTenantListQuery, PagedResultDto<TenantDto>>
{
    public async Task<PagedResultDto<TenantDto>> Handle(TenantsGetTenantListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.Tenants.AsNoTracking();
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(tenant => new TenantDto
            {
                Id = tenant.Id,
                Slug = tenant.Slug,
                Name = tenant.Name,
                Edition = tenant.Edition,
                App = tenant.App,
                ConnectionString = string.Empty,
                IsActive = tenant.IsActive,
                UserId = tenant.UserId
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TenantDto>(totalCount, items);
    }

    private static IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant> ApplySorting(
        IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(tenant => tenant.UpdatedDate);
        }

        IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>? orderedQuery = null;
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

        return orderedQuery ?? query.OrderByDescending(tenant => tenant.UpdatedDate);
    }

    private static IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant> ApplyOrder(
        IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "name" => isFirst
                ? (desc ? query.OrderByDescending(tenant => tenant.Name) : query.OrderBy(tenant => tenant.Name))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenByDescending(tenant => tenant.Name)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenBy(tenant => tenant.Name)),
            "slug" => isFirst
                ? (desc ? query.OrderByDescending(tenant => tenant.Slug) : query.OrderBy(tenant => tenant.Slug))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenByDescending(tenant => tenant.Slug)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenBy(tenant => tenant.Slug)),
            "edition" => isFirst
                ? (desc ? query.OrderByDescending(tenant => tenant.Edition) : query.OrderBy(tenant => tenant.Edition))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenByDescending(tenant => tenant.Edition)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenBy(tenant => tenant.Edition)),
            "isactive" => isFirst
                ? (desc ? query.OrderByDescending(tenant => tenant.IsActive) : query.OrderBy(tenant => tenant.IsActive))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenByDescending(tenant => tenant.IsActive)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenBy(tenant => tenant.IsActive)),
            "createddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(tenant => tenant.CreatedDate)
                    : query.OrderBy(tenant => tenant.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenByDescending(tenant => tenant.CreatedDate)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenBy(tenant => tenant.CreatedDate)),
            "updateddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(tenant => tenant.UpdatedDate)
                    : query.OrderBy(tenant => tenant.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenByDescending(tenant => tenant.UpdatedDate)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenBy(tenant => tenant.UpdatedDate)),
            "id" => isFirst
                ? (desc ? query.OrderByDescending(tenant => tenant.Id) : query.OrderBy(tenant => tenant.Id))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenByDescending(tenant => tenant.Id)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                    .ThenBy(tenant => tenant.Id)),
            _ => isFirst
                ? query.OrderByDescending(tenant => tenant.UpdatedDate)
                : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant>)query)
                .ThenByDescending(tenant => tenant.UpdatedDate)
        };
    }
}