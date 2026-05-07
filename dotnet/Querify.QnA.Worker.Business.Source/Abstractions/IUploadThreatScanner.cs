using Querify.QnA.Worker.Business.Source.Models;

namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface IUploadThreatScanner
{
    Task<UploadThreatScanResult> ScanAsync(Stream content, CancellationToken cancellationToken);
}
