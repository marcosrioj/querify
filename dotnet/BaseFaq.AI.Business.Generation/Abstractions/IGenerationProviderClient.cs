using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Generation.Models;

namespace BaseFaq.AI.Business.Generation.Abstractions;

public interface IGenerationProviderClient
{
    Task<GeneratedFaqDraft> GenerateDraftAsync(
        AiProviderContext providerContext,
        GenerationPromptData promptData,
        CancellationToken cancellationToken);
}