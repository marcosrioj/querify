using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Business.Source.BackgroundServices;

public sealed class SourceUploadVerificationBackgroundService(
    ISourceUploadVerificationSweepService verificationSweepService)
{
    public async Task VerifyUploadedSourcesAsync(CancellationToken cancellationToken)
    {
        await verificationSweepService.VerifyUploadedSourcesAsync(cancellationToken);
    }
}
