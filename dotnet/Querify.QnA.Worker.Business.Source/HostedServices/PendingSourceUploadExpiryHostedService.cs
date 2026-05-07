using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.Common.EntityFramework.Tenant;
using Querify.Models.Common.Enums;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploads;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Business.Source.HostedServices;

public sealed class PendingSourceUploadExpiryHostedService(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<PendingSourceUploadExpiryOptions> optionsMonitor,
    ILogger<PendingSourceUploadExpiryHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = optionsMonitor.CurrentValue;
            if (!options.Enabled)
            {
                await Task.Delay(options.PollingInterval, stoppingToken);
                continue;
            }

            try
            {
                await ExpireAllTenantsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception while expiring pending source uploads.");
            }

            await Task.Delay(options.PollingInterval, stoppingToken);
        }
    }

    private async Task ExpireAllTenantsAsync(CancellationToken cancellationToken)
    {
        await using var listScope = scopeFactory.CreateAsyncScope();
        var tenantDbContext = listScope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var tenantIds = await tenantDbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Module == ModuleEnum.QnA && tenant.IsActive)
            .OrderBy(tenant => tenant.Id)
            .Select(tenant => tenant.Id)
            .ToListAsync(cancellationToken);

        foreach (var tenantId in tenantIds)
        {
            await using var tenantScope = scopeFactory.CreateAsyncScope();
            var tenantContext = tenantScope.ServiceProvider.GetRequiredService<IQnAWorkerTenantContext>();
            using var activeTenant = tenantContext.UseTenant(tenantId);
            var mediator = tenantScope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ExpirePendingSourceUploadsCommand { NowUtc = DateTime.UtcNow },
                cancellationToken);
        }
    }
}
