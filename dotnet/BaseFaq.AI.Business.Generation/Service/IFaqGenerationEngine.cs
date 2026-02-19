using BaseFaq.AI.Business.Common.Models;
using BaseFaq.Models.Ai.Contracts.Generation;

namespace BaseFaq.AI.Business.Generation.Service;

public interface IFaqGenerationEngine
{
    GeneratedFaqDraft Generate(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        AiProviderContext providerContext);
}
