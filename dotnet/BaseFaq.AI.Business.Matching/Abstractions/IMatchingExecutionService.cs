using BaseFaq.Models.Ai.Contracts.Matching;

namespace BaseFaq.AI.Business.Matching.Abstractions;

public interface IMatchingExecutionService
{
    Task<MatchingCandidate[]> ExecuteAsync(FaqMatchingRequestedV1 message, CancellationToken cancellationToken);
}