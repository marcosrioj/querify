using MediatR;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploadsForAllTenants;
using Querify.QnA.Worker.Business.Source.Infrastructure;

namespace Querify.QnA.Worker.Business.Source.Services;

public sealed class PendingSourceUploadExpiryProcessorService(IMediator mediator)
    : IPendingSourceUploadExpiryProcessorService
{
    public async Task ExpireAllTenantsAsync(CancellationToken cancellationToken)
    {
        using var activity = SourceWorkerTelemetry.ActivitySource.StartActivity("source-upload-expiry.process-all-tenants");
        activity?.SetTag("worker.processor", "source-upload-expiry");

        var expiredAny = await mediator.Send(
            new ExpirePendingSourceUploadsForAllTenantsCommand { NowUtc = DateTime.UtcNow },
            cancellationToken);
        activity?.SetTag("worker.expired_any", expiredAny);
    }
}
