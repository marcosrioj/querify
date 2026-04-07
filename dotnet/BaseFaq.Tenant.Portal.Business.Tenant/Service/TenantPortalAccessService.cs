using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using EntityTenant = BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Service;

public class TenantPortalAccessService(
    TenantDbContext dbContext,
    ISessionService sessionService)
    : ITenantPortalAccessService
{
    public async Task EnsureAccessAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        _ = await GetAccessibleTenantAsync(tenantId, cancellationToken);
    }

    public async Task EnsureOwnerAccessAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        _ = await GetOwnedTenantWithUsersAsync(tenantId, cancellationToken);
    }

    public Task<EntityTenant> GetAccessibleTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return GetAccessibleTenantInternalAsync(tenantId, cancellationToken);
    }

    public Task<EntityTenant> GetAccessibleTenantWithUsersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return GetAccessibleTenantWithUsersInternalAsync(tenantId, cancellationToken);
    }

    public Task<EntityTenant> GetOwnedTenantWithUsersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return GetOwnedTenantWithUsersInternalAsync(tenantId, cancellationToken);
    }

    private async Task<EntityTenant> GetAccessibleTenantInternalAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await GetTenantOrThrowAsync(tenantId, includeTenantUsers: false, cancellationToken);
        await EnsureUserHasAccessAsync(tenantId, cancellationToken);
        return tenant;
    }

    private async Task<EntityTenant> GetAccessibleTenantWithUsersInternalAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await GetTenantOrThrowAsync(tenantId, includeTenantUsers: true, cancellationToken);
        EnsureUserHasAccessToLoadedTenant(tenant, requiresOwner: false);
        return tenant;
    }

    private async Task<EntityTenant> GetOwnedTenantWithUsersInternalAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await GetTenantOrThrowAsync(tenantId, includeTenantUsers: true, cancellationToken);
        EnsureUserHasAccessToLoadedTenant(tenant, requiresOwner: true);
        return tenant;
    }

    private async Task<EntityTenant> GetTenantOrThrowAsync(
        Guid tenantId,
        bool includeTenantUsers,
        CancellationToken cancellationToken)
    {
        IQueryable<EntityTenant> query = dbContext.Tenants;

        if (includeTenantUsers)
        {
            query = query.Include(entity => entity.TenantUsers);
        }

        var tenant = await query.FirstOrDefaultAsync(
            entity =>
                entity.Id == tenantId &&
                entity.IsActive,
            cancellationToken);

        if (tenant is not null)
        {
            return tenant;
        }

        throw new ApiErrorException(
            $"Tenant '{tenantId}' was not found.",
            errorCode: (int)HttpStatusCode.NotFound);
    }

    private async Task EnsureUserHasAccessAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var currentUserId = sessionService.GetUserId();
        var hasAccess = await dbContext.TenantUsers
            .AsNoTracking()
            .AnyAsync(
                entity => entity.TenantId == tenantId && entity.UserId == currentUserId,
                cancellationToken);

        if (hasAccess)
        {
            return;
        }

        throw new ApiErrorException(
            "The selected workspace is not available for the current user.",
            errorCode: (int)HttpStatusCode.Forbidden);
    }

    private void EnsureUserHasAccessToLoadedTenant(EntityTenant tenant, bool requiresOwner)
    {
        var currentUserId = sessionService.GetUserId();
        var tenantUser = tenant.TenantUsers.FirstOrDefault(entity => entity.UserId == currentUserId);

        if (tenantUser is null)
        {
            throw new ApiErrorException(
                "The selected workspace is not available for the current user.",
                errorCode: (int)HttpStatusCode.Forbidden);
        }

        if (!requiresOwner || tenantUser.Role == TenantUserRoleType.Owner)
        {
            return;
        }

        throw new ApiErrorException(
            "Only the workspace owner can manage members.",
            errorCode: (int)HttpStatusCode.Forbidden);
    }
}
