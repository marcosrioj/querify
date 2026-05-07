using MediatR;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Business.Source.Infrastructure;

namespace Querify.QnA.Worker.Business.Source.Services;

public sealed class SourceUploadVerificationService(
    IQnAWorkerTenantContext tenantContext,
    IMediator mediator)
    : ISourceUploadVerificationService
{
    public async Task VerifyUploadedAsync(
        Guid tenantId,
        Guid sourceId,
        string storageKey,
        CancellationToken cancellationToken)
    {
        using var activity = SourceWorkerTelemetry.ActivitySource.StartActivity("source-upload.verify");
        activity?.SetTag("source.id", sourceId);
        activity?.SetTag("tenant.id", tenantId);

        using var tenantScope = tenantContext.UseTenant(tenantId);
        await mediator.Send(new VerifyUploadedSourceCommand
        {
            TenantId = tenantId,
            SourceId = sourceId,
            StorageKey = storageKey
        }, cancellationToken);
    }
}
