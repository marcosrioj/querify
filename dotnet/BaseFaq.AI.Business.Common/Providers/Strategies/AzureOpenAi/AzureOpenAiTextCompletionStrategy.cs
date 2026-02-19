using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.AzureOpenAi;

public sealed class AzureOpenAiTextCompletionStrategy(ProviderHttpJsonClient httpClient)
    : IAiTextCompletionStrategy
{
    public AiProviderStyle Style => AiProviderStyle.AzureOpenAi;

    public async Task<AiTextCompletionResult> CompleteAsync(
        AiProviderRuntimeContext runtimeContext,
        AiTextCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var deployment = runtimeContext.Deployment ?? runtimeContext.ProviderContext.Model;

        var endpoint = ProviderEndpointBuilder.CombineWithQuery(
            runtimeContext.BaseUri,
            $"openai/deployments/{deployment}/chat/completions",
            new Dictionary<string, string?> { ["api-version"] = runtimeContext.ApiVersion });

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new ChatRequest(
                [
                    new ChatMessage("system", request.SystemPrompt),
                    new ChatMessage("user", request.UserPrompt)
                ],
                request.Temperature,
                new ResponseFormat("json_object")),
            message => message.Headers.Add("api-key", runtimeContext.ApiKey),
            cancellationToken);

        var response =
            JsonSerializer.Deserialize<ChatResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var content = response?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Azure OpenAI returned an empty completion payload.");
        }

        return new AiTextCompletionResult(content);
    }

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")]
        string Content);

    private sealed record ResponseFormat(
        [property: JsonPropertyName("type")] string Type);

    private sealed record ChatRequest(
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