using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Generation.Models;
using BaseFaq.Models.Ai.Contracts.Generation;

namespace BaseFaq.AI.Business.Generation.Abstractions;

public interface IGenerationPromptBuilder
{
    GenerationPromptData BuildPromptData(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        AiProviderContext providerContext);
}