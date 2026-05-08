using MediatR;
using Querify.Common.Infrastructure.Signalr.Portal.Abstractions;
using Querify.Common.Infrastructure.Signalr.Portal.Models;
using Querify.Models.Common.Enums;
using Querify.QnA.Portal.Business.Source.Notifications;

namespace Querify.QnA.Portal.Business.Source.Commands.NotifySourceUploadStatusChanged;

public sealed class NotifySourceUploadStatusChangedCommandHandler(
    IPortalNotificationPublisher notificationPublisher)
    : IRequestHandler<NotifySourceUploadStatusChangedCommand>
{
    public Task Handle(NotifySourceUploadStatusChangedCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var integrationEvent = request.IntegrationEvent;
        var envelope = new PortalNotificationEnvelope
        {
            NotificationId = Guid.NewGuid(),
            OccurredAtUtc = integrationEvent.OccurredAtUtc,
            Type = SourcePortalNotificationType.SourceUploadStatusChanged,
            Module = ModuleEnum.QnA.ToString(),
            TenantId = integrationEvent.TenantId,
            ResourceKind = "source",
            ResourceId = integrationEvent.SourceId,
            Version = 1,
            Payload = new SourceUploadStatusChangedNotificationPayload
            {
                UploadStatus = integrationEvent.UploadStatus,
                StorageKey = integrationEvent.StorageKey,
                Checksum = integrationEvent.Checksum,
                Reason = integrationEvent.Reason
            }
        };

        return notificationPublisher.PublishToTenantModuleAsync(
            ModuleEnum.QnA,
            integrationEvent.TenantId,
            envelope,
            cancellationToken);
    }
}
