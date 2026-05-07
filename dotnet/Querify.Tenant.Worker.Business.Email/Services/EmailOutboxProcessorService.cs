using Querify.Tenant.Worker.Business.Email.Abstractions;
using Querify.Tenant.Worker.Business.Email.Commands.ProcessEmailOutboxBatch;
using Querify.Tenant.Worker.Business.Email.Infrastructure;
using Querify.Tenant.Worker.Business.Email.Options;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Querify.Tenant.Worker.Business.Email.Services;

public sealed class EmailOutboxProcessorService(
    IMediator mediator,
    IOptionsMonitor<EmailProcessingOptions> optionsMonitor,
    ILogger<EmailOutboxProcessorService> logger)
    : IEmailOutboxProcessorService
{
    public async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.CurrentValue;

        using var activity = EmailWorkerTelemetry.ActivitySource.StartActivity("email-outbox.process-batch");
        activity?.SetTag("worker.processor", "email-outbox");
        activity?.SetTag("worker.batch_size", options.BatchSize);

        var processedAny = await mediator.Send(new ProcessEmailOutboxBatchCommand(), cancellationToken);
        activity?.SetTag("worker.processed_any", processedAny);
        logger.LogDebug("Email outbox batch processed any record: {ProcessedAny}.", processedAny);
        return processedAny;
    }
}
