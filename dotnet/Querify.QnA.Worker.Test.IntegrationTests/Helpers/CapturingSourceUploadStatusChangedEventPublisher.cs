using Querify.Models.QnA.Events;
using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Test.IntegrationTests.Helpers;

public sealed class CapturingSourceUploadStatusChangedEventPublisher : ISourceUploadStatusChangedEventPublisher
{
    public List<SourceUploadStatusChangedIntegrationEvent> PublishedEvents { get; } = [];

    public Task PublishAsync(
        SourceUploadStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        PublishedEvents.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
