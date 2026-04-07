using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.UpdateTenantUser;

public class TenantUsersUpdateTenantUserCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantUsersUpdateTenantUserCommand>
{
    public async Task Handle(TenantUsersUpdateTenantUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantPortalAccessService.GetAccessibleTenantWithUsersAsync(
            request.TenantId,
            cancellationToken);

        var tenantUser = tenant.TenantUsers.FirstOrDefault(entity => entity.Id == request.Id);
        if (tenantUser is null)
        {
            throw new ApiErrorException(
                $"Tenant user '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        if (tenantUser.Role == TenantUserRoleType.Owner && request.Role != TenantUserRoleType.Owner)
        {
            throw new ApiErrorException(
                "Promote another member to owner before demoting the current owner.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var previousOwnerUserId = TenantUserHelper.GetOwnerUserId(tenant.TenantUsers);

        if (request.Role == TenantUserRoleType.Owner)
        {
            TenantUserHelper.SetOwner(tenant.TenantUsers, tenant.Id, tenantUser.UserId);
        }
        else
        {
            tenantUser.Role = request.Role;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await AllowedTenantCacheHelper.RemoveUserEntries(
            allowedTenantStore,
            [tenantUser.UserId, previousOwnerUserId ?? Guid.Empty],
            cancellationToken);
    }
}
