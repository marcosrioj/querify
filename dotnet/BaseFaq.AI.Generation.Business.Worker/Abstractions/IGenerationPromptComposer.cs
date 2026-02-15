using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.AI.Common.Providers.Models;
using BaseFaq.AI.Generation.Business.Worker.Service;

namespace BaseFaq.AI.Generation.Business.Worker.Abstractions;

public interface IGenerationPromptComposer
{
    GenerationPromptComposition Compose(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        AiProviderCredential providerCredential);
}

public sealed record GenerationPromptComposition(
    string Domain,
    string Version,
    string Provider,
    string SystemPrompt,
    string UserPrompt,
    string ExpectedJsonSchema);