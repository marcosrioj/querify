namespace Querify.Models.QnA.Events;

public sealed class SourceUploadCompletedIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required DateTime OccurredAtUtc { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid SourceId { get; init; }
    public required string StorageKey { get; init; }
    public string? ClientChecksum { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public required string CompletedByUserId { get; init; }
}
