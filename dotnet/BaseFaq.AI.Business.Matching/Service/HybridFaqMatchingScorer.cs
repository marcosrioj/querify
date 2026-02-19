using System.Text.RegularExpressions;
using BaseFaq.Models.Ai.Contracts.Matching;

namespace BaseFaq.AI.Business.Matching.Service;

public sealed partial class HybridFaqMatchingScorer : IFaqMatchingScorer
{
    private const double TokenWeight = 0.75;
    private const double CharacterNGramWeight = 0.25;

    public MatchingCandidate[] Rank(string queryText, IReadOnlyCollection<CandidateQuestion> candidates, int maxCandidates)
    {
        return candidates
            .Select(x => new MatchingCandidate(x.Id, ComputeScore(queryText, x.Question)))
            .Where(x => x.SimilarityScore > 0)
            .OrderByDescending(x => x.SimilarityScore)
            .ThenBy(x => x.FaqItemId)
            .Take(Math.Max(1, maxCandidates))
            .ToArray();
    }

    private static double ComputeScore(string left, string right)
    {
        var tokenScore = Jaccard(Tokenize(left), Tokenize(right));
        var charScore = Jaccard(ToTrigrams(left), ToTrigrams(right));

        var weighted = tokenScore * TokenWeight + charScore * CharacterNGramWeight;
        return Math.Round(weighted, 4, MidpointRounding.AwayFromZero);
    }

    private static HashSet<string> Tokenize(string text)
    {
        return WordSplitter()
            .Split(text.Trim().ToLowerInvariant())
            .Where(x => x.Length > 1)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static HashSet<string> ToTrigrams(string text)
    {
        var normalized = string.Concat(text
            .Trim()
            .ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)));

        if (normalized.Length < 3)
        {
            return [];
        }

        var grams = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index <= normalized.Length - 3; index++)
        {
            grams.Add(normalized.Substring(index, 3));
        }

        return grams;
    }

    private static double Jaccard<T>(IReadOnlySet<T> left, IReadOnlySet<T> right)
    {
        if (left.Count == 0 || right.Count == 0)
        {
            return 0;
        }

        var intersection = left.Intersect(right).Count();
        if (intersection == 0)
        {
            return 0;
        }

        var union = left.Union(right).Count();
        return intersection / (double)union;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex WordSplitter();
}
