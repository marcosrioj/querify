using Querify.Models.QnA.Events;

namespace Querify.QnA.Portal.Business.Source.Abstractions;

public interface ISourceUploadCompletedEventPublisher
{
    Task PublishAsync(SourceUploadCompletedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
