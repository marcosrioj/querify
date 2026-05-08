using Querify.Models.QnA.Events;

namespace Querify.QnA.Portal.Business.Source.Notifications;

public interface ISourceUploadStatusChangedNotificationService
{
    Task NotifyAsync(
        SourceUploadStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
