using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.Cohere;

public sealed class CohereEmbeddingsStrategy(ProviderHttpJsonClient httpClient)
    : IAiEmbeddingsStrategy
{
    public AiProviderStyle Style => AiProviderStyle.Cohere;

    public async Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
        AiProviderRuntimeContext runtimeContext,
        AiEmbeddingsRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = ProviderEndpointBuilder.Combine(runtimeContext.BaseUri, "embed");

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new EmbeddingsRequest(runtimeContext.ProviderContext.Model, request.Inputs, "search_document"),
            message => message.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", runtimeContext.ApiKey),
            cancellationToken);

        var vectors = ParseEmbeddings(body);

        if (vectors.Count != request.Inputs.Count)
        {
            throw new InvalidOperationException("Cohere returned an invalid embeddings payload.");
        }

        return new AiEmbeddingsResult(vectors);
    }

    private static IReadOnlyList<float[]> ParseEmbeddings(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.TryGetProperty("embeddings", out var embeddingsProperty))
        {
            if (embeddingsProperty.ValueKind == JsonValueKind.Array)
            {
                return embeddingsProperty
                    .EnumerateArray()
                    .Select(ToFloatArray)
                    .ToArray();
            }

            if (embeddingsProperty.ValueKind == JsonValueKind.Object &&
                embeddingsProperty.TryGetProperty("float", out var floatArray) &&
                floatArray.ValueKind == JsonValueKind.Array)
            {
                return floatArray
                    .EnumerateArray()
                    .Select(ToFloatArray)
                    .ToArray();
            }
        }

        return [];
    }

    private static float[] ToFloatArray(JsonElement element)
    {
        return element
            .EnumerateArray()
            .Select(x => (float)x.GetDouble())
            .ToArray();
    }

    private sealed record EmbeddingsRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("texts")] IReadOnlyList<string> Texts,
        [property: JsonPropertyName("input_type")]
        string InputType);
}