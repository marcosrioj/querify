using BaseFaq.AI.Business.Matching.Service;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Matching.Infrastructure;

public sealed class HybridFaqMatchingScorerTests
{
    [Fact]
    public void Rank_PrioritizesMostRelevantQuestion()
    {
        var scorer = new HybridFaqMatchingScorer();

        var ranked = scorer.Rank(
            "how can i reset my account password",
            [
                new CandidateQuestion(Guid.NewGuid(), "How do I change my password for this account?"),
                new CandidateQuestion(Guid.NewGuid(), "How to contact support by phone"),
                new CandidateQuestion(Guid.NewGuid(), "Delete my account permanently")
            ],
            maxCandidates: 3);

        Assert.NotEmpty(ranked);
        Assert.True(ranked[0].SimilarityScore >= ranked[^1].SimilarityScore);
    }

    [Fact]
    public void Rank_ReturnsNoMatches_WhenQueryIsUnrelated()
    {
        var scorer = new HybridFaqMatchingScorer();

        var ranked = scorer.Rank(
            "volcanic ash cloud updates",
            [new CandidateQuestion(Guid.NewGuid(), "Password reset instructions")],
            maxCandidates: 5);

        Assert.Empty(ranked);
    }
}
