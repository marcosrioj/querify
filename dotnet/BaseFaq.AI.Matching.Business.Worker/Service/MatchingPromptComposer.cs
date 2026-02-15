using BaseFaq.AI.Common.Providers.Models;
using BaseFaq.AI.Matching.Business.Worker.Abstractions;
using System.Text;

namespace BaseFaq.AI.Matching.Business.Worker.Service;

public sealed class MatchingPromptComposer : IMatchingPromptComposer
{
    private const int MaxPromptCandidates = 100;
    private const string Domain = "matching";
    private const string PromptVersion = "2026-02-15.matching.v1";

    public MatchingPromptComposition Compose(
        string sourceQuestion,
        string queryText,
        string language,
        IReadOnlyCollection<MatchingPromptCandidate> candidates,
        AiProviderCredential providerCredential)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceQuestion);
        ArgumentException.ThrowIfNullOrWhiteSpace(queryText);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(providerCredential);

        var provider = NormalizeProvider(providerCredential.Provider);

        return new MatchingPromptComposition(
            Domain,
            PromptVersion,
            provider,
            BuildSystemPrompt(provider),
            BuildUserPrompt(sourceQuestion, queryText, language, candidates),
            BuildExpectedJsonSchema());
    }

    private static string BuildSystemPrompt(string provider)
    {
        var providerDirective = provider switch
        {
            "openai" => "Prioritize deterministic ranking and calibrated score spread.",
            "anthropic" => "Explain uncertainty briefly and avoid overconfident high scores.",
            "google" => "Optimize semantic relevance over lexical overlap.",
            _ => "Apply strict ranking rules and deterministic JSON output."
        };

        return string.Join(Environment.NewLine, [
            "You are a FAQ semantic matching engine.",
            "Rank candidates by semantic intent similarity to the query.",
            "Do not use tenant IDs or FAQ IDs as ranking clues.",
            "Output only valid JSON following the schema.",
            providerDirective
        ]);
    }

    private static string BuildUserPrompt(
        string sourceQuestion,
        string queryText,
        string language,
        IReadOnlyCollection<MatchingPromptCandidate> candidates)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Task: rank FAQ candidates by semantic similarity.");
        builder.AppendLine($"language: {language}");
        builder.AppendLine($"sourceQuestion: {sourceQuestion}");
        builder.AppendLine($"query: {queryText}");
        builder.AppendLine($"candidatesTotal: {candidates.Count}");
        builder.AppendLine("candidates:");

        foreach (var candidate in candidates.Take(MaxPromptCandidates))
        {
            builder.AppendLine($"- faqItemId={candidate.FaqItemId:D}, question={candidate.Question}");
        }

        builder.AppendLine("Return top 5 candidates with score in [0,1], sorted descending.");
        return builder.ToString();
    }

    private static string BuildExpectedJsonSchema()
    {
        return """
               {
                 "type": "object",
                 "required": ["topCandidates"],
                 "properties": {
                   "topCandidates": {
                     "type": "array",
                     "maxItems": 5,
                     "items": {
                       "type": "object",
                       "required": ["faqItemId", "score", "reason"],
                       "properties": {
                         "faqItemId": { "type": "string" },
                         "score": { "type": "number", "minimum": 0, "maximum": 1 },
                         "reason": { "type": "string", "maxLength": 300 }
                       }
                     }
                   }
                 }
               }
               """;
    }

    private static string NormalizeProvider(string? provider)
    {
        return string.IsNullOrWhiteSpace(provider)
            ? "unknown"
            : provider.Trim().ToLowerInvariant();
    }
}