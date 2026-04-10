using BaseFaq.Tenant.Worker.Business.Billing.Abstractions;
using BaseFaq.Tenant.Worker.Business.Billing.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BaseFaq.Tenant.Worker.Business.Billing.HostedServices;

public sealed class BillingWebhookInboxProcessorHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptionsMonitor<BillingProcessingOptions> optionsMonitor,
    ILogger<BillingWebhookInboxProcessorHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Billing webhook inbox processor hosted service started.");

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
                var processor = scope.ServiceProvider.GetRequiredService<IBillingWebhookInboxProcessor>();
                var claimedCount = await processor.ProcessBatchAsync(stoppingToken);
                var delay = claimedCount > 0 ? TimeSpan.Zero : options.PollingInterval;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Billing webhook inbox processor loop failed.");
                await Task.Delay(options.PollingInterval, stoppingToken);
            }
        }

        logger.LogInformation("Billing webhook inbox processor hosted service stopped.");
    }
}
