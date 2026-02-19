using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Abstractions;

public interface IAiEmbeddingsGateway
{
    Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
        AiProviderRuntimeContext runtimeContext,
        AiEmbeddingsRequest request,
        CancellationToken cancellationToken);
}