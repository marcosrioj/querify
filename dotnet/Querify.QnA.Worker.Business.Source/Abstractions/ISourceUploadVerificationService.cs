namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface ISourceUploadVerificationService
{
    Task VerifyUploadedAsync(Guid tenantId, Guid sourceId, string storageKey, CancellationToken cancellationToken);
}
