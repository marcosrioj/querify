using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Helpers;

internal static class TenantAccessHelper
{
    public static Task<bool> HasAccessAsync(
        TenantDbContext dbContext,
        Guid tenantId,
        Guid userId,
        AppEnum app,
        CancellationToken cancellationToken)
    {
        return dbContext.TenantUsers
            .AsNoTracking()
            .AnyAsync(
                tenantUser =>
                    tenantUser.TenantId == tenantId &&
                    tenantUser.UserId == userId &&
                    tenantUser.Tenant.IsActive &&
                    tenantUser.Tenant.App == app,
                cancellationToken);
    }

    public static async Task EnsureAccessAsync(
        TenantDbContext dbContext,
        Guid tenantId,
        Guid userId,
        AppEnum app,
        CancellationToken cancellationToken)
    {
        if (await HasAccessAsync(dbContext, tenantId, userId, app, cancellationToken))
        {
            return;
        }

        throw new ApiErrorException(
            "The selected workspace is not available for the current user.",
            errorCode: (int)HttpStatusCode.Forbidden);
    }
}
