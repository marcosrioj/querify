using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.AzureOpenAi;

public sealed class AzureOpenAiEmbeddingsStrategy(ProviderHttpJsonClient httpClient)
    : IAiEmbeddingsStrategy
{
    public AiProviderStyle Style => AiProviderStyle.AzureOpenAi;

    public async Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
        AiProviderRuntimeContext runtimeContext,
        AiEmbeddingsRequest request,
        CancellationToken cancellationToken)
    {
        var deployment = runtimeContext.Deployment ?? runtimeContext.ProviderContext.Model;

        var endpoint = ProviderEndpointBuilder.CombineWithQuery(
            runtimeContext.BaseUri,
            $"openai/deployments/{deployment}/embeddings",
            new Dictionary<string, string?> { ["api-version"] = runtimeContext.ApiVersion });

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new EmbeddingsRequest(request.Inputs),
            message => message.Headers.Add("api-key", runtimeContext.ApiKey),
            cancellationToken);

        var response =
            JsonSerializer.Deserialize<EmbeddingsResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var vectors = response?.Data?
            .OrderBy(x => x.Index)
            .Select(x => x.Embedding)
            .ToArray();

        if (vectors is null || vectors.Length != request.Inputs.Count)
        {
            throw new InvalidOperationException("Azure OpenAI returned an invalid embeddings payload.");
        }

        return new AiEmbeddingsResult(vectors);
    }

    private sealed record EmbeddingsRequest([property: JsonPropertyName("input")] IReadOnlyList<string> Input);

    private sealed record EmbeddingsResponse(
        [property: JsonPropertyName("data")] IReadOnlyList<EmbeddingData>? Data);

    private sealed record EmbeddingData(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("embedding")]
        float[] Embedding);
}