using MediatR;
using Microsoft.Extensions.Options;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSourcesForAllTenants;
using Querify.QnA.Worker.Business.Source.Infrastructure;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Business.Source.Services;

public sealed class SourceUploadVerificationSweepService(
    IMediator mediator,
    IOptionsMonitor<SourceUploadVerificationSweepOptions> optionsMonitor)
    : ISourceUploadVerificationSweepService
{
    public async Task<bool> VerifyUploadedSourcesAsync(CancellationToken cancellationToken)
    {
        var options = optionsMonitor.CurrentValue;
        using var activity = SourceWorkerTelemetry.ActivitySource.StartActivity("source-upload.verify-uploaded-sources");
        activity?.SetTag("worker.processor", "source-upload-verification");
        activity?.SetTag("worker.enabled", options.Enabled);
        activity?.SetTag("worker.batch_size", options.BatchSize);

        if (!options.Enabled)
        {
            activity?.SetTag("worker.processed_any", false);
            return false;
        }

        var processedAny = await mediator.Send(
            new VerifyUploadedSourcesForAllTenantsCommand { BatchSize = options.BatchSize },
            cancellationToken);

        activity?.SetTag("worker.processed_any", processedAny);
        return processedAny;
    }
}
