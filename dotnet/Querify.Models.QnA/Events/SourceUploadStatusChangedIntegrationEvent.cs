using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Events;

public sealed class SourceUploadStatusChangedIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required DateTime OccurredAtUtc { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid SourceId { get; init; }
    public required SourceUploadStatus UploadStatus { get; init; }
    public string? StorageKey { get; init; }
    public string? Checksum { get; init; }
    public string? Reason { get; init; }
}
