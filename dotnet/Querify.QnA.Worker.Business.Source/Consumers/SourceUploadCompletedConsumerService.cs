using MediatR;
using Querify.Models.QnA.Events;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Business.Source.Infrastructure;

namespace Querify.QnA.Worker.Business.Source.Consumers;

public sealed class SourceUploadCompletedConsumerService(
    IQnAWorkerTenantContext tenantContext,
    IMediator mediator)
    : ISourceUploadCompletedConsumerService
{
    public async Task ProcessAsync(
        SourceUploadCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        using var activity = SourceWorkerTelemetry.ActivitySource.StartActivity("source-upload-completed.consume");
        activity?.SetTag("source.id", integrationEvent.SourceId);
        activity?.SetTag("tenant.id", integrationEvent.TenantId);

        using var tenantScope = tenantContext.UseTenant(integrationEvent.TenantId);
        await mediator.Send(new VerifyUploadedSourceCommand
        {
            TenantId = integrationEvent.TenantId,
            SourceId = integrationEvent.SourceId,
            StorageKey = integrationEvent.StorageKey
        }, cancellationToken);
    }
}
