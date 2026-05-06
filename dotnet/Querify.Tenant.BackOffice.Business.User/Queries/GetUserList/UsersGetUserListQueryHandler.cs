using Querify.Common.EntityFramework.Tenant;
using Querify.Models.Common.Dtos;
using Querify.Models.User.Dtos.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.BackOffice.Business.User.Queries.GetUserList;

public class UsersGetUserListQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<UsersGetUserListQuery, PagedResultDto<UserDto>>
{
    public async Task<PagedResultDto<UserDto>> Handle(UsersGetUserListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.Users.AsNoTracking();
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(user => new UserDto
            {
                Id = user.Id,
                GivenName = user.GivenName,
                SurName = user.SurName,
                Email = user.Email,
                ExternalId = user.ExternalId,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<UserDto>(totalCount, items);
    }

    private static IQueryable<Querify.Common.EntityFramework.Tenant.Entities.User> ApplySorting(
        IQueryable<Querify.Common.EntityFramework.Tenant.Entities.User> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(user => user.UpdatedDate);
        }

        IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>? orderedQuery = null;
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

        return orderedQuery ?? query.OrderByDescending(user => user.UpdatedDate);
    }

    private static IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User> ApplyOrder(
        IQueryable<Querify.Common.EntityFramework.Tenant.Entities.User> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "givenname" => isFirst
                ? (desc ? query.OrderByDescending(user => user.GivenName) : query.OrderBy(user => user.GivenName))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.GivenName)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.GivenName)),
            "surname" => isFirst
                ? (desc ? query.OrderByDescending(user => user.SurName) : query.OrderBy(user => user.SurName))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.SurName)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.SurName)),
            "email" => isFirst
                ? (desc ? query.OrderByDescending(user => user.Email) : query.OrderBy(user => user.Email))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.Email)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.Email)),
            "externalid" => isFirst
                ? (desc ? query.OrderByDescending(user => user.ExternalId) : query.OrderBy(user => user.ExternalId))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.ExternalId)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.ExternalId)),
            "phonenumber" => isFirst
                ? (desc ? query.OrderByDescending(user => user.PhoneNumber) : query.OrderBy(user => user.PhoneNumber))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.PhoneNumber)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.PhoneNumber)),
            "role" => isFirst
                ? (desc ? query.OrderByDescending(user => user.Role) : query.OrderBy(user => user.Role))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.Role)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.Role)),
            "createddate" => isFirst
                ? (desc ? query.OrderByDescending(user => user.CreatedDate) : query.OrderBy(user => user.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.CreatedDate)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.CreatedDate)),
            "updateddate" => isFirst
                ? (desc ? query.OrderByDescending(user => user.UpdatedDate) : query.OrderBy(user => user.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.UpdatedDate)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.UpdatedDate)),
            "id" => isFirst
                ? (desc ? query.OrderByDescending(user => user.Id) : query.OrderBy(user => user.Id))
                : (desc
                    ? ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenByDescending(user => user.Id)
                    : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                    .ThenBy(user => user.Id)),
            _ => isFirst
                ? query.OrderByDescending(user => user.UpdatedDate)
                : ((IOrderedQueryable<Querify.Common.EntityFramework.Tenant.Entities.User>)query)
                .ThenByDescending(user => user.UpdatedDate)
        };
    }
}