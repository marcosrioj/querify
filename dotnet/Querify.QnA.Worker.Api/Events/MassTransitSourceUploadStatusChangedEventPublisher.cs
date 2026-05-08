using MassTransit;
using Querify.Models.QnA.Events;
using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Api.Events;

public sealed class MassTransitSourceUploadStatusChangedEventPublisher(IPublishEndpoint publishEndpoint)
    : ISourceUploadStatusChangedEventPublisher
{
    public Task PublishAsync(
        SourceUploadStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
