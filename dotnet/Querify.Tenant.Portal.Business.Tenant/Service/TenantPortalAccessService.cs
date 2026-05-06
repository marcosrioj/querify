using System.Net;
using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using EntityTenant = Querify.Common.EntityFramework.Tenant.Entities.Tenant;

namespace Querify.Tenant.Portal.Business.Tenant.Service;

public class TenantPortalAccessService(
    TenantDbContext dbContext,
    ISessionService sessionService)
    : ITenantPortalAccessService
{
    public async Task EnsureAccessAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await EnsureActiveTenantExistsAsync(tenantId, cancellationToken);
        await EnsureUserHasAccessAsync(tenantId, requiresOwner: false, cancellationToken);
    }

    public async Task EnsureOwnerAccessAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await EnsureActiveTenantExistsAsync(tenantId, cancellationToken);
        await EnsureUserHasAccessAsync(tenantId, requiresOwner: true, cancellationToken);
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
        await EnsureUserHasAccessAsync(tenantId, requiresOwner: false, cancellationToken);
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

    private async Task EnsureActiveTenantExistsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenantExists = await dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(
                entity =>
                    entity.Id == tenantId &&
                    entity.IsActive,
                cancellationToken);

        if (tenantExists)
        {
            return;
        }

        throw new ApiErrorException(
            $"Tenant '{tenantId}' was not found.",
            errorCode: (int)HttpStatusCode.NotFound);
    }

    private async Task EnsureUserHasAccessAsync(
        Guid tenantId,
        bool requiresOwner,
        CancellationToken cancellationToken)
    {
        var currentUserId = sessionService.GetUserId();

        var role = await dbContext.TenantUsers
            .AsNoTracking()
            .Where(entity => entity.TenantId == tenantId && entity.UserId == currentUserId)
            .Select(entity => (TenantUserRoleType?)entity.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            throw new ApiErrorException(
                "The selected workspace is not available for the current user.",
                errorCode: (int)HttpStatusCode.Forbidden);
        }

        if (!requiresOwner || role == TenantUserRoleType.Owner)
        {
            return;
        }

        throw new ApiErrorException(
            "Only the workspace owner can manage members.",
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
