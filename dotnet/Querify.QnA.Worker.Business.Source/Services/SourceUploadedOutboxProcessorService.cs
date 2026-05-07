using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.ProcessSourceUploadedOutbox;
using Querify.QnA.Worker.Business.Source.Infrastructure;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Business.Source.Services;

public sealed class SourceUploadedOutboxProcessorService(
    IMediator mediator,
    IOptionsMonitor<SourceUploadedOutboxProcessingOptions> optionsMonitor,
    ILogger<SourceUploadedOutboxProcessorService> logger)
    : ISourceUploadedOutboxProcessorService
{
    public async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        using var activity = SourceWorkerTelemetry.ActivitySource.StartActivity("source-upload-outbox.process-batch");
        activity?.SetTag("worker.processor", "source-upload-outbox");
        activity?.SetTag("worker.batch_size", optionsMonitor.CurrentValue.BatchSize);

        var processedAny = await mediator.Send(new ProcessSourceUploadedOutboxCommand(), cancellationToken);

        activity?.SetTag("worker.processed_any", processedAny);
        logger.LogDebug("Source upload outbox batch processed any message: {ProcessedAny}.", processedAny);
        return processedAny;
    }
}
