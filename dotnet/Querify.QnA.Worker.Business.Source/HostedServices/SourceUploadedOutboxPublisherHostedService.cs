using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Business.Source.HostedServices;

public sealed class SourceUploadedOutboxPublisherHostedService(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<SourceUploadedOutboxProcessingOptions> optionsMonitor,
    ILogger<SourceUploadedOutboxPublisherHostedService> logger)
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
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<ISourceUploadedOutboxProcessor>();
                var processed = await processor.ProcessBatchAsync(stoppingToken);
                if (processed > 0)
                {
                    continue;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception while processing source upload outbox messages.");
            }

            await Task.Delay(options.PollingInterval, stoppingToken);
        }
    }
}
