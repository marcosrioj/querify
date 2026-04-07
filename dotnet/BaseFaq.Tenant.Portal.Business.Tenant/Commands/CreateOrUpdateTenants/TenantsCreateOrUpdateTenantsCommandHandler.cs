using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;

public class TenantsCreateOrUpdateTenantsCommandHandler(
    TenantDbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor,
    IAllowedTenantStore allowedTenantStore)
    : IRequestHandler<TenantsCreateOrUpdateTenantsCommand, bool>
{
    public async Task<bool> Handle(TenantsCreateOrUpdateTenantsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();
        var selectedTenantId = TryGetSelectedTenantId(httpContextAccessor);

        var currentConnections = await GetCurrentConnectionsAsync(cancellationToken);
        if (selectedTenantId.HasValue)
        {
            await UpdateSelectedTenantAsync(
                request,
                selectedTenantId.Value,
                userId,
                currentConnections,
                cancellationToken);
        }
        else
        {
            await CreateActiveTenantsAsync(request, userId, currentConnections, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!selectedTenantId.HasValue)
        {
            await AllowedTenantCacheHelper.RemoveUserEntries(allowedTenantStore, [userId], cancellationToken);
        }

        return true;
    }

    private async Task<Dictionary<AppEnum, string>> GetCurrentConnectionsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TenantConnections
            .AsNoTracking()
            .Where(entity => entity.IsCurrent)
            .ToDictionaryAsync(entity => entity.App, entity => entity.ConnectionString, cancellationToken);
    }

    private async Task CreateActiveTenantsAsync(
        TenantsCreateOrUpdateTenantsCommand request,
        Guid userId,
        IReadOnlyDictionary<AppEnum, string> currentConnections,
        CancellationToken cancellationToken)
    {
        var apps = Enum.GetValues<AppEnum>().Where(app => app != AppEnum.Tenant);

        foreach (var app in apps)
        {
            if (!TryGetConnectionString(currentConnections, app, out var connectionString))
            {
                continue;
            }

            await CreateActiveTenantAsync(request, userId, app, connectionString, cancellationToken);
        }
    }

    private static bool TryGetConnectionString(
        IReadOnlyDictionary<AppEnum, string> currentConnections,
        AppEnum app,
        out string connectionString)
    {
        if (!currentConnections.TryGetValue(app, out connectionString!) || string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = string.Empty;
            return false;
        }

        return true;
    }

    private async Task CreateActiveTenantAsync(
        TenantsCreateOrUpdateTenantsCommand request,
        Guid userId,
        AppEnum app,
        string connectionString,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.NewGuid();

        var tenant = new BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant
        {
            Id = tenantId,
            Slug = await GenerateUniqueSlugAsync(request.Name, app, tenantId: null, cancellationToken),
            Name = request.Name,
            Edition = request.Edition,
            App = app,
            ConnectionString = connectionString,
            IsActive = true
        };
        TenantUserHelper.SetOwner(tenant.TenantUsers, tenantId, userId);

        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    private async Task UpdateSelectedTenantAsync(
        TenantsCreateOrUpdateTenantsCommand request,
        Guid tenantId,
        Guid userId,
        IReadOnlyDictionary<AppEnum, string> currentConnections,
        CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants
            .FirstOrDefaultAsync(entity => entity.Id == tenantId && entity.IsActive, cancellationToken);
        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        await TenantAccessHelper.EnsureOwnerAsync(dbContext, tenantId, userId, cancellationToken);

        tenant.Slug = await GenerateUniqueSlugAsync(request.Name, tenant.App, tenant.Id, cancellationToken);
        tenant.Name = request.Name;
        tenant.Edition = request.Edition;

        if (TryGetConnectionString(currentConnections, tenant.App, out var connectionString))
        {
            tenant.ConnectionString = connectionString;
        }

        tenant.IsActive = true;
        dbContext.Tenants.Update(tenant);
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string tenantName,
        AppEnum app,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var baseSlug = TenantHelper.GenerateSlug($"{tenantName}{app}");
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = $"tenant{Guid.NewGuid():N}";
        }

        var candidate = baseSlug;
        var counter = 2;

        while (await dbContext.Tenants
                   .AsNoTracking()
                   .AnyAsync(entity => entity.Id != tenantId && entity.Slug == candidate, cancellationToken))
        {
            candidate = $"{baseSlug}{counter}";
            counter++;
        }

        return candidate;
    }

    private static Guid? TryGetSelectedTenantId(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.Items.TryGetValue(TenantContextKeys.TenantIdItemKey, out var value) != true)
        {
            return null;
        }

        return value as Guid? ?? (value is Guid tenantId ? tenantId : null);
    }
}
