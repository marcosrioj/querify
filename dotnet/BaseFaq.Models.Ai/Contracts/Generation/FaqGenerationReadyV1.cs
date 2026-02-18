namespace BaseFaq.Models.Ai.Contracts.Generation;

public sealed record FaqGenerationReadyV1
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required Guid JobId { get; init; }
    public required Guid FaqId { get; init; }
    public required Guid TenantId { get; init; }
    public required DateTime OccurredUtc { get; init; }
}