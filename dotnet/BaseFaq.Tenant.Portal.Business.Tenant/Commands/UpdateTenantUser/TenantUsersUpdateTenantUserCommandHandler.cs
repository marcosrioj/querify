using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.UpdateTenantUser;

public class TenantUsersUpdateTenantUserCommandHandler(
    TenantDbContext dbContext,
    ISessionService sessionService,
    IAllowedTenantStore allowedTenantStore)
    : IRequestHandler<TenantUsersUpdateTenantUserCommand>
{
    public async Task Handle(TenantUsersUpdateTenantUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = sessionService.GetUserId();
        var tenantId = request.TenantId;
        await TenantAccessHelper.EnsureAccessAsync(dbContext, tenantId, currentUserId, AppEnum.Faq, cancellationToken);

        var tenant = await dbContext.Tenants
            .Include(entity => entity.TenantUsers)
            .FirstOrDefaultAsync(entity => entity.Id == tenantId && entity.IsActive, cancellationToken);

        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

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
