namespace Querify.QnA.Common.Domain.Options;

public sealed class SourceUploadOptions
{
    public const string SectionName = "SourceUpload";
    public const long DefaultMaxUploadBytes = 52_428_800;
    public const int DefaultPendingExpirationHours = 24;

    public long MaxUploadBytes { get; set; } = DefaultMaxUploadBytes;

    public int PendingExpirationHours { get; set; } = DefaultPendingExpirationHours;

    public string? ThreatScanningMode { get; set; }

    public string[] AllowedContentTypes { get; set; } =
    [
        "application/pdf",
        "image/png",
        "image/jpeg",
        "video/mp4",
        "text/plain",
        "text/markdown"
    ];
}
