using BaseFaq.AI.Common.Providers.Models;

namespace BaseFaq.AI.Matching.Business.Worker.Abstractions;

public interface IMatchingPromptComposer
{
    MatchingPromptComposition Compose(
        string sourceQuestion,
        string queryText,
        string language,
        IReadOnlyCollection<MatchingPromptCandidate> candidates,
        AiProviderCredential providerCredential);
}

public sealed record MatchingPromptCandidate(Guid FaqItemId, string Question);

public sealed record MatchingPromptComposition(
    string Domain,
    string Version,
    string Provider,
    string SystemPrompt,
    string UserPrompt,
    string ExpectedJsonSchema);