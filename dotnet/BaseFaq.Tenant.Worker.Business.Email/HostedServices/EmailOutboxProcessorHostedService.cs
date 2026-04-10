using BaseFaq.Tenant.Worker.Business.Email.Abstractions;
using BaseFaq.Tenant.Worker.Business.Email.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BaseFaq.Tenant.Worker.Business.Email.HostedServices;

public sealed class EmailOutboxProcessorHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptionsMonitor<EmailProcessingOptions> optionsMonitor,
    ILogger<EmailOutboxProcessorHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email outbox processor hosted service started.");

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
                var processor = scope.ServiceProvider.GetRequiredService<IEmailOutboxProcessor>();
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
                logger.LogError(ex, "Email outbox processor loop failed.");
                await Task.Delay(options.PollingInterval, stoppingToken);
            }
        }

        logger.LogInformation("Email outbox processor hosted service stopped.");
    }
}
