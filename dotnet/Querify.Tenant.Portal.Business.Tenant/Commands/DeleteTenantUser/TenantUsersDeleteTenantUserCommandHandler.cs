using System.Net;
using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Helpers;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.DeleteTenantUser;

public class TenantUsersDeleteTenantUserCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantUsersDeleteTenantUserCommand>
{
    public async Task Handle(TenantUsersDeleteTenantUserCommand request, CancellationToken cancellationToken)
    {
        await tenantPortalAccessService.EnsureOwnerAccessAsync(request.TenantId, cancellationToken);

        var tenantUser = await dbContext.TenantUsers
            .FirstOrDefaultAsync(entity => entity.Id == request.Id && entity.TenantId == request.TenantId, cancellationToken);

        if (tenantUser is null)
        {
            throw new ApiErrorException(
                $"Tenant user '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        if (tenantUser.Role == TenantUserRoleType.Owner)
        {
            throw new ApiErrorException(
                "The workspace owner cannot be removed.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        dbContext.TenantUsers.Remove(tenantUser);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AllowedTenantCacheHelper.RemoveUserEntries(allowedTenantStore, [tenantUser.UserId], cancellationToken);
    }
}
