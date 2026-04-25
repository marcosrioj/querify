using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.TenantConnection;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnectionList;

public class TenantConnectionsGetTenantConnectionListQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantConnectionsGetTenantConnectionListQuery, PagedResultDto<TenantConnectionDto>>
{
    public async Task<PagedResultDto<TenantConnectionDto>> Handle(TenantConnectionsGetTenantConnectionListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.TenantConnections.AsNoTracking();
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(connection => new TenantConnectionDto
            {
                Id = connection.Id,
                Module = connection.Module,
                ConnectionString = string.Empty,
                IsCurrent = connection.IsCurrent
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TenantConnectionDto>(totalCount, items);
    }

    private static IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection> ApplySorting(
        IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(connection => connection.UpdatedDate);
        }

        IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>? orderedQuery = null;
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

        return orderedQuery ?? query.OrderByDescending(connection => connection.UpdatedDate);
    }

    private static IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection> ApplyOrder(
        IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "module" => isFirst
                ? (desc
                    ? query.OrderByDescending(connection => connection.Module)
                    : query.OrderBy(connection => connection.Module))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenByDescending(connection => connection.Module)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenBy(connection => connection.Module)),
            "iscurrent" => isFirst
                ? (desc
                    ? query.OrderByDescending(connection => connection.IsCurrent)
                    : query.OrderBy(connection => connection.IsCurrent))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenByDescending(connection => connection.IsCurrent)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenBy(connection => connection.IsCurrent)),
            "createddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(connection => connection.CreatedDate)
                    : query.OrderBy(connection => connection.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenByDescending(connection => connection.CreatedDate)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenBy(connection => connection.CreatedDate)),
            "updateddate" => isFirst
                ? (desc
                    ? query.OrderByDescending(connection => connection.UpdatedDate)
                    : query.OrderBy(connection => connection.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenByDescending(connection => connection.UpdatedDate)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenBy(connection => connection.UpdatedDate)),
            "id" => isFirst
                ? (desc
                    ? query.OrderByDescending(connection => connection.Id)
                    : query.OrderBy(connection => connection.Id))
                : (desc
                    ? ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenByDescending(connection => connection.Id)
                    : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                    .ThenBy(connection => connection.Id)),
            _ => isFirst
                ? query.OrderByDescending(connection => connection.UpdatedDate)
                : ((IOrderedQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.TenantConnection>)query)
                .ThenByDescending(connection => connection.UpdatedDate)
        };
    }
}