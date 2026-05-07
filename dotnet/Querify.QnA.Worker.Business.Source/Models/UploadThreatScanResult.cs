namespace Querify.QnA.Worker.Business.Source.Models;

public sealed record UploadThreatScanResult(bool IsSafe, string? Reason)
{
    public static UploadThreatScanResult Safe() => new(true, null);

    public static UploadThreatScanResult Unsafe(string reason) => new(false, reason);
}
