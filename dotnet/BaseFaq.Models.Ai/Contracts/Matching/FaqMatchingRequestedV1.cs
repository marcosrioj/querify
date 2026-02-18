namespace BaseFaq.Models.Ai.Contracts.Matching;

public sealed record FaqMatchingRequestedV1
{
    public required Guid CorrelationId { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid FaqItemId { get; init; }
    public required Guid RequestedByUserId { get; init; }
    public required string Query { get; init; }
    public required string Language { get; init; }
    public required string IdempotencyKey { get; init; }
    public required DateTime RequestedUtc { get; init; }
}