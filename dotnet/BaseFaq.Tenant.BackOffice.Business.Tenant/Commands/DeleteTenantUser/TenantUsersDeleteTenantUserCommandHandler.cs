using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using BaseFaq.Models.Tenant.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantUser;

public class TenantUsersDeleteTenantUserCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore)
    : IRequestHandler<TenantUsersDeleteTenantUserCommand>
{
    public async Task Handle(TenantUsersDeleteTenantUserCommand request, CancellationToken cancellationToken)
    {
        var tenantUser = await dbContext.TenantUsers
            .FirstOrDefaultAsync(
                entity => entity.Id == request.Id && entity.TenantId == request.TenantId,
                cancellationToken);

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
