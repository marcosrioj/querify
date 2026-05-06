using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Tenant.Dtos.TenantUser;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;

public class TenantUsersGetTenantUserListQueryHandler(
    TenantDbContext dbContext,
    ISessionService sessionService,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantUsersGetTenantUserListQuery, List<TenantUserDto>>
{
    public async Task<List<TenantUserDto>> Handle(TenantUsersGetTenantUserListQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = sessionService.GetUserId();
        await tenantPortalAccessService.EnsureAccessAsync(request.TenantId, cancellationToken);

        return await dbContext.TenantUsers
            .AsNoTracking()
            .Where(tenantUser => tenantUser.TenantId == request.TenantId)
            .OrderBy(tenantUser => tenantUser.Role == TenantUserRoleType.Owner ? 0 : 1)
            .ThenBy(tenantUser => tenantUser.User.Email)
            .Select(tenantUser => new TenantUserDto
            {
                Id = tenantUser.Id,
                TenantId = tenantUser.TenantId,
                UserId = tenantUser.UserId,
                GivenName = tenantUser.User.GivenName,
                SurName = tenantUser.User.SurName,
                Email = tenantUser.User.Email,
                Role = tenantUser.Role,
                IsCurrentUser = tenantUser.UserId == currentUserId
            })
            .ToListAsync(cancellationToken);
    }
}
