using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Models;

namespace Querify.QnA.Worker.Test.IntegrationTests.Helpers;

public sealed class FakeThreatScanner(bool isSafe = true) : IUploadThreatScanner
{
    public Task<UploadThreatScanResult> ScanAsync(Stream content, CancellationToken cancellationToken)
    {
        return Task.FromResult(isSafe
            ? UploadThreatScanResult.Safe()
            : UploadThreatScanResult.Unsafe("test threat"));
    }
}
