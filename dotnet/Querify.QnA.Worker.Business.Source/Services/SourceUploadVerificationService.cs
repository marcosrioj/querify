using MediatR;
using Querify.Models.QnA.Dtos.IntegrationEvents;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Business.Source.Infrastructure;

namespace Querify.QnA.Worker.Business.Source.Services;

public sealed class SourceUploadVerificationService(
    IQnAWorkerTenantContext tenantContext,
    IMediator mediator)
    : ISourceUploadVerificationService
{
    public async Task VerifyUploadedAsync(SourceUploadedIntegrationEvent message, CancellationToken cancellationToken)
    {
        using var activity = SourceWorkerTelemetry.ActivitySource.StartActivity("source-upload.verify");
        activity?.SetTag("source.id", message.SourceId);
        activity?.SetTag("tenant.id", message.TenantId);

        using var tenantScope = tenantContext.UseTenant(message.TenantId);
        await mediator.Send(new VerifyUploadedSourceCommand
        {
            TenantId = message.TenantId,
            SourceId = message.SourceId,
            StorageKey = message.StorageKey,
            ClientChecksum = message.ClientChecksum,
            UploadedAtUtc = message.UploadedAtUtc
        }, cancellationToken);
    }
}
