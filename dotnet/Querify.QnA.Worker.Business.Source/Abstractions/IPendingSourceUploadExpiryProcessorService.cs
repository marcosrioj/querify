namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface IPendingSourceUploadExpiryProcessorService
{
    Task ExpireAllTenantsAsync(CancellationToken cancellationToken);
}
