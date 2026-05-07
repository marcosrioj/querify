using Querify.Models.QnA.Dtos.IntegrationEvents;

namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface ISourceUploadVerificationService
{
    Task VerifyUploadedAsync(SourceUploadedIntegrationEvent message, CancellationToken cancellationToken);
}
