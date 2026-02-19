using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Infrastructure;

public sealed class AiProviderProfileRegistry : IAiProviderProfileRegistry
{
    private static readonly IReadOnlyDictionary<string, AiProviderProfile> Profiles =
        new Dictionary<string, AiProviderProfile>(StringComparer.Ordinal)
        {
            [AiProviderNames.OpenAi] = new(AiProviderNames.OpenAi, AiProviderStyle.OpenAiCompatible,
                "https://api.openai.com/v1", true, true),

            [AiProviderNames.Anthropic] = new(AiProviderNames.Anthropic, AiProviderStyle.Anthropic,
                "https://api.anthropic.com/v1", true, true),

            [AiProviderNames.Google] = new(AiProviderNames.Google, AiProviderStyle.Google,
                "https://generativelanguage.googleapis.com/v1beta", true, true),

            [AiProviderNames.AzureOpenAi] = new(AiProviderNames.AzureOpenAi, AiProviderStyle.AzureOpenAi,
                null, true, true,
                "Azure OpenAI requires endpoint in credential format: https://resource.openai.azure.com|api-key|optional-deployment|optional-api-version."),

            [AiProviderNames.AwsBedrock] = new(AiProviderNames.AwsBedrock, AiProviderStyle.Unsupported,
                null, false, false,
                "AWS Bedrock is not supported by this worker runtime because it requires SigV4 credentials and region configuration."),

            [AiProviderNames.Cohere] = new(AiProviderNames.Cohere, AiProviderStyle.Cohere,
                "https://api.cohere.com/v2", true, true),

            [AiProviderNames.Mistral] = new(AiProviderNames.Mistral, AiProviderStyle.OpenAiCompatible,
                "https://api.mistral.ai/v1", true, true),

            [AiProviderNames.TogetherAi] = new(AiProviderNames.TogetherAi, AiProviderStyle.OpenAiCompatible,
                "https://api.together.xyz/v1", true, true),

            [AiProviderNames.FireworksAi] = new(AiProviderNames.FireworksAi, AiProviderStyle.OpenAiCompatible,
                "https://api.fireworks.ai/inference/v1", true, true),

            [AiProviderNames.Groq] = new(AiProviderNames.Groq, AiProviderStyle.OpenAiCompatible,
                "https://api.groq.com/openai/v1", true, true),

            [AiProviderNames.VoyageAi] = new(AiProviderNames.VoyageAi, AiProviderStyle.Voyage,
                "https://api.voyageai.com/v1", false, true,
                "Voyage supports embeddings; generation requires external LLM provider."),

            [AiProviderNames.JinaAi] = new(AiProviderNames.JinaAi, AiProviderStyle.OpenAiCompatible,
                "https://api.jina.ai/v1", false, true,
                "Jina supports embeddings; generation requires external LLM provider.")
        };

    public AiProviderProfile Resolve(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException("Provider name is required.");
        }

        var normalizedProvider = provider.Trim().ToLowerInvariant();

        if (Profiles.TryGetValue(normalizedProvider, out var profile))
        {
            return profile;
        }

        throw new NotSupportedException($"Provider '{provider}' is not recognized by this worker runtime.");
    }
}