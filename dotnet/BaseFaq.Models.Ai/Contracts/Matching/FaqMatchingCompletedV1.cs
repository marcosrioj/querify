namespace BaseFaq.Models.Ai.Contracts.Matching;

public sealed record FaqMatchingCompletedV1
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid FaqItemId { get; init; }
    public required IReadOnlyCollection<MatchingCandidate> Candidates { get; init; }
    public required DateTime OccurredUtc { get; init; }
}

public sealed record MatchingCandidate(Guid FaqItemId, double SimilarityScore);