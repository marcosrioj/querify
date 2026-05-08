using Querify.Models.QnA.Events;

namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface ISourceUploadCompletedConsumerService
{
    Task ProcessAsync(
        SourceUploadCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
