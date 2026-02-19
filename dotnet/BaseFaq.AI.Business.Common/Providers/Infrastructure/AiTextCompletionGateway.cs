using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Infrastructure;

public sealed class AiTextCompletionGateway(IEnumerable<IAiTextCompletionStrategy> strategies)
    : IAiTextCompletionGateway
{
    private readonly IReadOnlyDictionary<AiProviderStyle, IAiTextCompletionStrategy> _strategyByStyle =
        strategies.ToDictionary(x => x.Style);

    public Task<AiTextCompletionResult> CompleteAsync(
        AiProviderRuntimeContext runtimeContext,
        AiTextCompletionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(runtimeContext);
        ArgumentNullException.ThrowIfNull(request);

        if (!_strategyByStyle.TryGetValue(runtimeContext.Profile.Style, out var strategy))
        {
            throw new NotSupportedException(
                $"No text completion strategy registered for style '{runtimeContext.Profile.Style}' and provider '{runtimeContext.Profile.Name}'.");
        }

        return strategy.CompleteAsync(runtimeContext, request, cancellationToken);
    }
}