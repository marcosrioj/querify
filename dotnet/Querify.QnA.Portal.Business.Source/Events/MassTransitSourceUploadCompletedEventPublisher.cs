using MassTransit;
using Querify.Models.QnA.Events;
using Querify.QnA.Portal.Business.Source.Abstractions;

namespace Querify.QnA.Portal.Business.Source.Events;

public sealed class MassTransitSourceUploadCompletedEventPublisher(IPublishEndpoint publishEndpoint)
    : ISourceUploadCompletedEventPublisher
{
    public Task PublishAsync(
        SourceUploadCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
