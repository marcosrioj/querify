using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.Voyage;

public sealed class VoyageEmbeddingsStrategy(ProviderHttpJsonClient httpClient)
    : IAiEmbeddingsStrategy
{
    public AiProviderStyle Style => AiProviderStyle.Voyage;

    public async Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
        AiProviderRuntimeContext runtimeContext,
        AiEmbeddingsRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = ProviderEndpointBuilder.Combine(runtimeContext.BaseUri, "embeddings");

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new EmbeddingsRequest(runtimeContext.ProviderContext.Model, request.Inputs),
            message => message.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", runtimeContext.ApiKey),
            cancellationToken);

        var response =
            JsonSerializer.Deserialize<EmbeddingsResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var vectors = response?.Data?
            .Select(x => x.Embedding)
            .ToArray();

        if (vectors is null || vectors.Length != request.Inputs.Count)
        {
            throw new InvalidOperationException("Voyage returned an invalid embeddings payload.");
        }

        return new AiEmbeddingsResult(vectors);
    }

    private sealed record EmbeddingsRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] IReadOnlyList<string> Input);

    private sealed record EmbeddingsResponse([property: JsonPropertyName("data")] IReadOnlyList<EmbeddingData>? Data);

    private sealed record EmbeddingData(
        [property: JsonPropertyName("embedding")]
        float[] Embedding);
}