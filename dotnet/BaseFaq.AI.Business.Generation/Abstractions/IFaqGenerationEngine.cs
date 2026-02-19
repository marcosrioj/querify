using BaseFaq.Models.Ai.Contracts.Generation;
using BaseFaq.AI.Business.Generation.Dtos;

namespace BaseFaq.AI.Business.Generation.Abstractions;

public interface IFaqGenerationEngine
{
    GeneratedFaqDraft Generate(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs);
}