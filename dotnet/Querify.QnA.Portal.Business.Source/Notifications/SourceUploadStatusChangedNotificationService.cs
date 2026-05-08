using MediatR;
using Querify.Models.QnA.Events;
using Querify.QnA.Portal.Business.Source.Commands.NotifySourceUploadStatusChanged;
using Querify.QnA.Portal.Business.Source.Infrastructure;

namespace Querify.QnA.Portal.Business.Source.Notifications;

public sealed class SourceUploadStatusChangedNotificationService(IMediator mediator)
    : ISourceUploadStatusChangedNotificationService
{
    public async Task NotifyAsync(
        SourceUploadStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        using var activity = SourcePortalTelemetry.ActivitySource.StartActivity("source-upload-status-changed.notify");
        activity?.SetTag("tenant.id", integrationEvent.TenantId);
        activity?.SetTag("source.id", integrationEvent.SourceId);
        activity?.SetTag("source.upload_status", integrationEvent.UploadStatus.ToString());

        await mediator.Send(new NotifySourceUploadStatusChangedCommand
        {
            IntegrationEvent = integrationEvent
        }, cancellationToken);
    }
}
