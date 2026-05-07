using Querify.Tenant.Worker.Business.Billing.Abstractions;
using Querify.Tenant.Worker.Business.Billing.Commands.ProcessBillingWebhookInboxBatch;
using Querify.Tenant.Worker.Business.Billing.Infrastructure;
using Querify.Tenant.Worker.Business.Billing.Options;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Querify.Tenant.Worker.Business.Billing.Services;

public sealed class BillingWebhookInboxProcessorService(
    IMediator mediator,
    IOptionsMonitor<BillingProcessingOptions> optionsMonitor,
    ILogger<BillingWebhookInboxProcessorService> logger)
    : IBillingWebhookInboxProcessorService
{
    public async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.CurrentValue;

        using var activity = BillingWorkerTelemetry.ActivitySource.StartActivity("billing-webhook-inbox.process-batch");
        activity?.SetTag("worker.processor", "billing-webhook-inbox");
        activity?.SetTag("worker.batch_size", options.BatchSize);

        var processedAny = await mediator.Send(new ProcessBillingWebhookInboxBatchCommand(), cancellationToken);
        activity?.SetTag("worker.processed_any", processedAny);
        logger.LogDebug("Billing webhook inbox batch processed any record: {ProcessedAny}.", processedAny);
        return processedAny;
    }
}
