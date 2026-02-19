using BaseFaq.Models.Ai.Contracts.Generation;

namespace BaseFaq.AI.Business.Generation.Abstractions;

public interface IGenerationExecutionService
{
    Task ExecuteAsync(FaqGenerationRequestedV1 message, CancellationToken cancellationToken);
}