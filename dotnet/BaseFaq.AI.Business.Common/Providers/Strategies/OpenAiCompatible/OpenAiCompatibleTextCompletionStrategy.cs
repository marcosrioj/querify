using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.OpenAiCompatible;

public sealed class OpenAiCompatibleTextCompletionStrategy(ProviderHttpJsonClient httpClient)
    : IAiTextCompletionStrategy
{
    public AiProviderStyle Style => AiProviderStyle.OpenAiCompatible;

    public async Task<AiTextCompletionResult> CompleteAsync(
        AiProviderRuntimeContext runtimeContext,
        AiTextCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = ProviderEndpointBuilder.Combine(runtimeContext.BaseUri, "chat/completions");

        var payload = new ChatRequestWithResponseFormat(
            runtimeContext.ProviderContext.Model,
            [
                new ChatMessage("system", request.SystemPrompt),
                new ChatMessage("user", request.UserPrompt)
            ],
            request.Temperature,
            new ResponseFormat("json_object"));

        var body = await PostWithCompatibilityFallbackAsync(runtimeContext, endpoint, payload, cancellationToken);
        var content = ParseCompletionContent(body);
        return new AiTextCompletionResult(content);
    }

    private async Task<string> PostWithCompatibilityFallbackAsync(
        AiProviderRuntimeContext runtimeContext,
        Uri endpoint,
        ChatRequestWithResponseFormat payload,
        CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.PostJsonAsync(
                endpoint,
                payload,
                request => request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", runtimeContext.ApiKey),
                cancellationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("response_format",
                                                       StringComparison.OrdinalIgnoreCase))
        {
            var fallbackPayload = new ChatRequest(
                payload.Model,
                payload.Messages,
                payload.Temperature);

            return await httpClient.PostJsonAsync(
                endpoint,
                fallbackPayload,
                request => request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", runtimeContext.ApiKey),
                cancellationToken);
        }
    }

    private static string ParseCompletionContent(string body)
    {
        var response =
            JsonSerializer.Deserialize<ChatResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var content = response?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Provider returned an empty text completion payload.");
        }

        return content;
    }

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")]
        string Content);

    private sealed record ResponseFormat(
        [property: JsonPropertyName("type")] string Type);

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")]
        IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")]
        double Temperature);

    private sealed record ChatRequestWithResponseFormat(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")]
        IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")]
        double Temperature,
        [property: JsonPropertyName("response_format")]
        ResponseFormat ResponseFormat);

    private sealed record ChatResponse(
        [property: JsonPropertyName("choices")]
        IReadOnlyList<Choice>? Choices);

    private sealed record Choice(
        [property: JsonPropertyName("message")]
        ChoiceMessage? Message);

    private sealed record ChoiceMessage(
        [property: JsonPropertyName("content")]
        string? Content);
}