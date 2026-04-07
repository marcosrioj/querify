using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Tenant.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Helpers;

internal static class TenantAccessHelper
{
    public static Task<bool> IsOwnerAsync(
        TenantDbContext dbContext,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return dbContext.TenantUsers
            .AsNoTracking()
            .AnyAsync(
                tenantUser =>
                    tenantUser.TenantId == tenantId &&
                    tenantUser.UserId == userId &&
                    tenantUser.Role == TenantUserRoleType.Owner &&
                    tenantUser.Tenant.IsActive,
                cancellationToken);
    }

    public static async Task EnsureOwnerAsync(
        TenantDbContext dbContext,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (await IsOwnerAsync(dbContext, tenantId, userId, cancellationToken))
        {
            return;
        }

        throw new ApiErrorException(
            "Only workspace owners can manage this workspace.",
            errorCode: (int)HttpStatusCode.Forbidden);
    }
}
