namespace BaseFaq.Models.Ai.Contracts.Matching;

public sealed record FaqMatchingFailedV1
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid FaqItemId { get; init; }
    public required string ErrorCode { get; init; }
    public required string ErrorMessage { get; init; }
    public required DateTime OccurredUtc { get; init; }
}