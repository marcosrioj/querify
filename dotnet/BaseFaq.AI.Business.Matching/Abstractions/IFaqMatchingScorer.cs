using BaseFaq.Models.Ai.Contracts.Matching;
using BaseFaq.AI.Business.Matching.Dtos;

namespace BaseFaq.AI.Business.Matching.Abstractions;

public interface IFaqMatchingScorer
{
    MatchingCandidate[] Rank(string queryText, IReadOnlyCollection<CandidateQuestion> candidates, int maxCandidates);
}