using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Helpers;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Helpers;
using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;

public class TenantsCreateOrUpdateTenantsCommandHandler(
    TenantDbContext dbContext,
    ISessionService sessionService,
    IAllowedTenantStore allowedTenantStore,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantsCreateOrUpdateTenantsCommand, bool>
{
    public async Task<bool> Handle(TenantsCreateOrUpdateTenantsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();
        var selectedTenantId = request.TenantId;

        var currentConnections = await GetCurrentConnectionsAsync(cancellationToken);
        if (selectedTenantId.HasValue)
        {
            await UpdateSelectedTenantAsync(
                request,
                selectedTenantId.Value,
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

    private async Task<Dictionary<ModuleEnum, string>> GetCurrentConnectionsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TenantConnections
            .AsNoTracking()
            .Where(entity => entity.IsCurrent)
            .ToDictionaryAsync(entity => entity.Module, entity => entity.ConnectionString, cancellationToken);
    }

    private async Task CreateActiveTenantsAsync(
        TenantsCreateOrUpdateTenantsCommand request,
        Guid userId,
        IReadOnlyDictionary<ModuleEnum, string> currentConnections,
        CancellationToken cancellationToken)
    {
        var modules = Enum.GetValues<ModuleEnum>().Where(module => module != ModuleEnum.Tenant);

        foreach (var module in modules)
        {
            if (!TryGetConnectionString(currentConnections, module, out var connectionString))
            {
                continue;
            }

            await CreateActiveTenantAsync(request, userId, module, connectionString, cancellationToken);
        }
    }

    private static bool TryGetConnectionString(
        IReadOnlyDictionary<ModuleEnum, string> currentConnections,
        ModuleEnum module,
        out string connectionString)
    {
        if (!currentConnections.TryGetValue(module, out connectionString!) || string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = string.Empty;
            return false;
        }

        return true;
    }

    private async Task CreateActiveTenantAsync(
        TenantsCreateOrUpdateTenantsCommand request,
        Guid userId,
        ModuleEnum module,
        string connectionString,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.NewGuid();

        var tenant = new Querify.Common.EntityFramework.Tenant.Entities.Tenant
        {
            Id = tenantId,
            Slug = await GenerateUniqueSlugAsync(request.Name, module, tenantId: null, cancellationToken),
            Name = request.Name,
            Edition = request.Edition,
            Module = module,
            ConnectionString = connectionString,
            IsActive = true
        };
        TenantUserHelper.SetOwner(tenant.TenantUsers, tenantId, userId);

        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    private async Task UpdateSelectedTenantAsync(
        TenantsCreateOrUpdateTenantsCommand request,
        Guid tenantId,
        IReadOnlyDictionary<ModuleEnum, string> currentConnections,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantPortalAccessService.GetAccessibleTenantAsync(tenantId, cancellationToken);

        tenant.Slug = await GenerateUniqueSlugAsync(request.Name, tenant.Module, tenant.Id, cancellationToken);
        tenant.Name = request.Name;
        tenant.Edition = request.Edition;

        if (TryGetConnectionString(currentConnections, tenant.Module, out var connectionString))
        {
            tenant.ConnectionString = connectionString;
        }

        tenant.IsActive = true;
        dbContext.Tenants.Update(tenant);
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string tenantName,
        ModuleEnum module,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var baseSlug = TenantHelper.GenerateSlug($"{tenantName}{module}");
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
}
