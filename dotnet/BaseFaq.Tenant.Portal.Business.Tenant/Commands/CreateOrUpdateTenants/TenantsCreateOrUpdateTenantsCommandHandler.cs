using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;

public class TenantsCreateOrUpdateTenantsCommandHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsCreateOrUpdateTenantsCommand, bool>
{
    public async Task<bool> Handle(TenantsCreateOrUpdateTenantsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();

        var currentConnections = await GetCurrentConnectionsAsync(cancellationToken);
        await UpsertActiveTenantsAsync(request, userId, currentConnections, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<Dictionary<AppEnum, string>> GetCurrentConnectionsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TenantConnections
            .AsNoTracking()
            .Where(entity => entity.IsCurrent)
            .ToDictionaryAsync(entity => entity.App, entity => entity.ConnectionString, cancellationToken);
    }

    private async Task UpsertActiveTenantsAsync(
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

            await UpsertActiveTenantAsync(request, userId, app, connectionString, cancellationToken);
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

    private async Task UpsertActiveTenantAsync(
        TenantsCreateOrUpdateTenantsCommand request,
        Guid userId,
        AppEnum app,
        string connectionString,
        CancellationToken cancellationToken)
    {
        var activeTenant = await dbContext.Tenants
            .FirstOrDefaultAsync(
                entity => entity.UserId == userId && entity.App == app && entity.IsActive,
                cancellationToken);

        var slug = TenantHelper.GenerateSlug($"{request.Name}{app}");

        if (activeTenant is null)
        {
            var aiProviderIdDefault = await GetAiProviderIdDefault(app, cancellationToken);

            await dbContext.Tenants.AddAsync(
                new Common.EntityFramework.Tenant.Entities.Tenant
                {
                    Slug = slug,
                    Name = request.Name,
                    Edition = request.Edition,
                    App = app,
                    ConnectionString = connectionString,
                    AiProviderId = aiProviderIdDefault,
                    IsActive = true,
                    UserId = userId
                },
                cancellationToken);
            return;
        }

        activeTenant.Slug = slug;
        activeTenant.Name = request.Name;
        activeTenant.Edition = request.Edition;

        dbContext.Tenants.Update(activeTenant);
    }

    private async Task<Guid> GetAiProviderIdDefault(AppEnum app, CancellationToken cancellationToken)
    {
        var aiProviderId = await dbContext.AiProviders
            .Where(x => x.Provider.ToLower() == "openai")
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (aiProviderId == Guid.Empty)
        {
            var aiProviderIdDefaul = await dbContext.AiProviders
                .OrderByDescending(x => x.UpdatedDate)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (aiProviderIdDefaul == Guid.Empty)
            {
                throw new ApiErrorException(
                    "Default Ai Provider was not found.",
                    errorCode: (int)HttpStatusCode.InternalServerError);
            }

            return aiProviderIdDefaul;
        }

        return aiProviderId;
    }
}