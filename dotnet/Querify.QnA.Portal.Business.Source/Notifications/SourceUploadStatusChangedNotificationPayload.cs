using Querify.Models.QnA.Enums;

namespace Querify.QnA.Portal.Business.Source.Notifications;

public sealed class SourceUploadStatusChangedNotificationPayload
{
    public required SourceUploadStatus UploadStatus { get; init; }
    public string? StorageKey { get; init; }
    public string? Checksum { get; init; }
    public string? Reason { get; init; }
}
