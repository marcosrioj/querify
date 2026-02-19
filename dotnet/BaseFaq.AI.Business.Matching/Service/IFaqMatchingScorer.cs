using BaseFaq.Models.Ai.Contracts.Matching;

namespace BaseFaq.AI.Business.Matching.Service;

public interface IFaqMatchingScorer
{
    MatchingCandidate[] Rank(string queryText, IReadOnlyCollection<CandidateQuestion> candidates, int maxCandidates);
}

public sealed record CandidateQuestion(Guid Id, string Question);
