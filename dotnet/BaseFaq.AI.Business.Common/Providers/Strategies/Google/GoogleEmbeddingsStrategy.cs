using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.Google;

public sealed class GoogleEmbeddingsStrategy(ProviderHttpJsonClient httpClient)
    : IAiEmbeddingsStrategy
{
    public AiProviderStyle Style => AiProviderStyle.Google;

    public async Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
        AiProviderRuntimeContext runtimeContext,
        AiEmbeddingsRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = ProviderEndpointBuilder.CombineWithQuery(
            runtimeContext.BaseUri,
            $"models/{runtimeContext.ProviderContext.Model}:batchEmbedContents",
            new Dictionary<string, string?> { ["key"] = runtimeContext.ApiKey });

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new BatchEmbedRequest(
                request.Inputs
                    .Select(text => new EmbedRequestContent(new EmbedContent([new EmbedPart(text)])))
                    .ToArray()),
            _ => { },
            cancellationToken);

        var response =
            JsonSerializer.Deserialize<BatchEmbedResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var vectors = response?.Embeddings?
            .Select(x => x.Values)
            .ToArray();

        if (vectors is null || vectors.Length != request.Inputs.Count)
        {
            throw new InvalidOperationException("Google returned an invalid embeddings payload.");
        }

        return new AiEmbeddingsResult(vectors);
    }

    private sealed record EmbedPart([property: JsonPropertyName("text")] string Text);

    private sealed record EmbedContent([property: JsonPropertyName("parts")] IReadOnlyList<EmbedPart> Parts);

    private sealed record EmbedRequestContent(
        [property: JsonPropertyName("content")]
        EmbedContent Content);

    private sealed record BatchEmbedRequest(
        [property: JsonPropertyName("requests")]
        IReadOnlyList<EmbedRequestContent> Requests);

    private sealed record BatchEmbedResponse(
        [property: JsonPropertyName("embeddings")]
        IReadOnlyList<EmbeddingItem>? Embeddings);

    private sealed record EmbeddingItem([property: JsonPropertyName("values")] float[] Values);
}