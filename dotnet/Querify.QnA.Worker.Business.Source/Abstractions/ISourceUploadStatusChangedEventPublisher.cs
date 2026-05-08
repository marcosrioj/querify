using Querify.Models.QnA.Events;

namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface ISourceUploadStatusChangedEventPublisher
{
    Task PublishAsync(SourceUploadStatusChangedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
