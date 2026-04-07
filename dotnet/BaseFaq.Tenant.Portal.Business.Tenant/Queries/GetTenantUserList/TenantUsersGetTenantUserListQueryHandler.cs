using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Dtos.TenantUser;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;

public class TenantUsersGetTenantUserListQueryHandler(
    TenantDbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<TenantUsersGetTenantUserListQuery, List<TenantUserDto>>
{
    public async Task<List<TenantUserDto>> Handle(TenantUsersGetTenantUserListQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = sessionService.GetUserId();
        var tenantId = request.TenantId;
        await TenantAccessHelper.EnsureAccessAsync(dbContext, tenantId, currentUserId, AppEnum.Faq, cancellationToken);

        return await dbContext.TenantUsers
            .AsNoTracking()
            .Where(tenantUser => tenantUser.TenantId == tenantId)
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
