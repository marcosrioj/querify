using BaseFaq.AI.Business.Common.Models;
using BaseFaq.Models.Ai.Contracts.Matching;

namespace BaseFaq.AI.Business.Matching.Abstractions;

public interface IMatchingProviderClient
{
    Task<MatchingCandidate[]> RankAsync(
        AiProviderContext providerContext,
        string queryText,
        IReadOnlyList<(Guid Id, string Question)> candidates,
        int maxCandidates,
        CancellationToken cancellationToken);
}