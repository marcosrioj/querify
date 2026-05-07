namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface ISourceUploadedOutboxProcessorService
{
    Task<bool> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
