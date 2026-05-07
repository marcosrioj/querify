using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Business.Source.HostedServices;

public sealed class PendingSourceUploadExpiryHostedService(
    IPendingSourceUploadExpiryProcessorService expiryProcessorService,
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
    }
}
