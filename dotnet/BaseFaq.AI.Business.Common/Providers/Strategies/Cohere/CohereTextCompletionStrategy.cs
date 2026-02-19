using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.Cohere;

public sealed class CohereTextCompletionStrategy(ProviderHttpJsonClient httpClient)
    : IAiTextCompletionStrategy
{
    public AiProviderStyle Style => AiProviderStyle.Cohere;

    public async Task<AiTextCompletionResult> CompleteAsync(
        AiProviderRuntimeContext runtimeContext,
        AiTextCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = ProviderEndpointBuilder.Combine(runtimeContext.BaseUri, "chat");

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new ChatRequest(
                runtimeContext.ProviderContext.Model,
                [
                    new ChatMessage("system", request.SystemPrompt),
                    new ChatMessage("user", request.UserPrompt)
                ],
                request.Temperature),
            message => message.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", runtimeContext.ApiKey),
            cancellationToken);

        var text = ParseResponseText(body);

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Cohere returned an empty completion payload.");
        }

        return new AiTextCompletionResult(text);
    }

    private static string? ParseResponseText(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.TryGetProperty("text", out var textProperty))
        {
            return textProperty.GetString();
        }

        if (root.TryGetProperty("message", out var messageProperty) &&
            messageProperty.TryGetProperty("content", out var contentArray) &&
            contentArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in contentArray.EnumerateArray())
            {
                if (item.TryGetProperty("text", out var itemText) && itemText.ValueKind == JsonValueKind.String)
                {
                    return itemText.GetString();
                }
            }
        }

        return null;
    }

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")]
        string Content);

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")]
        IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")]
        double Temperature);
}