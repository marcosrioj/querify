namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface ISourceUploadVerificationSweepService
{
    Task<bool> VerifyUploadedSourcesAsync(CancellationToken cancellationToken);
}
