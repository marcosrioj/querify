namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface ISourceUploadedOutboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
