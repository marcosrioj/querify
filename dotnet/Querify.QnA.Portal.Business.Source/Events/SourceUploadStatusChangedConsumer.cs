using MassTransit;
using Querify.Models.QnA.Events;
using Querify.QnA.Portal.Business.Source.Notifications;

namespace Querify.QnA.Portal.Business.Source.Events;

public sealed class SourceUploadStatusChangedConsumer(
    ISourceUploadStatusChangedNotificationService notificationService)
    : IConsumer<SourceUploadStatusChangedIntegrationEvent>
{
    public Task Consume(ConsumeContext<SourceUploadStatusChangedIntegrationEvent> context)
    {
        return notificationService.NotifyAsync(context.Message, context.CancellationToken);
    }
}
