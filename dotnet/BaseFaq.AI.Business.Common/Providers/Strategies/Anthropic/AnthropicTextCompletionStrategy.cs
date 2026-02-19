using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.Anthropic;

public sealed class AnthropicTextCompletionStrategy(ProviderHttpJsonClient httpClient)
    : IAiTextCompletionStrategy
{
    private const string AnthropicVersion = "2023-06-01";

    public AiProviderStyle Style => AiProviderStyle.Anthropic;

    public async Task<AiTextCompletionResult> CompleteAsync(
        AiProviderRuntimeContext runtimeContext,
        AiTextCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = ProviderEndpointBuilder.Combine(runtimeContext.BaseUri, "messages");

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new MessagesRequest(
                runtimeContext.ProviderContext.Model,
                1400,
                request.SystemPrompt,
                [new AnthropicMessage("user", request.UserPrompt)],
                request.Temperature),
            message =>
            {
                message.Headers.Add("x-api-key", runtimeContext.ApiKey);
                message.Headers.Add("anthropic-version", AnthropicVersion);
            },
            cancellationToken);

        var response =
            JsonSerializer.Deserialize<MessagesResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var content = response?.Content?.FirstOrDefault(x => x.Type == "text")?.Text;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Anthropic returned an empty completion payload.");
        }

        return new AiTextCompletionResult(content);
    }

    private sealed record AnthropicMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")]
        string Content);

    private sealed record MessagesRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")]
        int MaxTokens,
        [property: JsonPropertyName("system")] string System,
        [property: JsonPropertyName("messages")]
        IReadOnlyList<AnthropicMessage> Messages,
        [property: JsonPropertyName("temperature")]
        double Temperature);

    private sealed record MessagesResponse(
        [property: JsonPropertyName("content")]
        IReadOnlyList<MessageContent>? Content);

    private sealed record MessageContent(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("text")] string? Text);
}