using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Business.Source.HostedServices;

public sealed class PendingSourceUploadExpiryHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptionsMonitor<PendingSourceUploadExpiryOptions> optionsMonitor,
    ILogger<PendingSourceUploadExpiryHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Pending source upload expiry hosted service started.");

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
                using var scope = serviceScopeFactory.CreateScope();
                var expiryProcessorService =
                    scope.ServiceProvider.GetRequiredService<IPendingSourceUploadExpiryProcessorService>();
                await expiryProcessorService.ExpireAllTenantsAsync(stoppingToken);
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

        logger.LogInformation("Pending source upload expiry hosted service stopped.");
    }
}
