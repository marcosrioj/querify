using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Models;

namespace Querify.QnA.Worker.Business.Source.Services;

public sealed class NoopUploadThreatScanner : IUploadThreatScanner
{
    public Task<UploadThreatScanResult> ScanAsync(Stream content, CancellationToken cancellationToken)
    {
        return Task.FromResult(UploadThreatScanResult.Safe());
    }
}
