using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Infrastructure;

public sealed class AiEmbeddingsGateway(IEnumerable<IAiEmbeddingsStrategy> strategies)
    : IAiEmbeddingsGateway
{
    private readonly IReadOnlyDictionary<AiProviderStyle, IAiEmbeddingsStrategy> _strategyByStyle =
        strategies.ToDictionary(x => x.Style);

    public Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
        AiProviderRuntimeContext runtimeContext,
        AiEmbeddingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(runtimeContext);
        ArgumentNullException.ThrowIfNull(request);

        if (!_strategyByStyle.TryGetValue(runtimeContext.Profile.Style, out var strategy))
        {
            throw new NotSupportedException(
                $"No embeddings strategy registered for style '{runtimeContext.Profile.Style}' and provider '{runtimeContext.Profile.Name}'.");
        }

        return strategy.CreateEmbeddingsAsync(runtimeContext, request, cancellationToken);
    }
}