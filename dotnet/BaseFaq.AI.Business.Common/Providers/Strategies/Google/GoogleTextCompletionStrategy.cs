using System.Text.Json;
using System.Text.Json.Serialization;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Strategies.Google;

public sealed class GoogleTextCompletionStrategy(ProviderHttpJsonClient httpClient)
    : IAiTextCompletionStrategy
{
    public AiProviderStyle Style => AiProviderStyle.Google;

    public async Task<AiTextCompletionResult> CompleteAsync(
        AiProviderRuntimeContext runtimeContext,
        AiTextCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = ProviderEndpointBuilder.CombineWithQuery(
            runtimeContext.BaseUri,
            $"models/{runtimeContext.ProviderContext.Model}:generateContent",
            new Dictionary<string, string?> { ["key"] = runtimeContext.ApiKey });

        var body = await httpClient.PostJsonAsync(
            endpoint,
            new GenerateContentRequest(
                new SystemInstruction([new TextPart(request.SystemPrompt)]),
                [new Content("user", [new TextPart(request.UserPrompt)])],
                new GenerationConfig(request.Temperature)),
            _ => { },
            cancellationToken);

        var response =
            JsonSerializer.Deserialize<GenerateContentResponse>(body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var text = response?.Candidates?
            .FirstOrDefault()?
            .Content?
            .Parts?
            .Select(x => x.Text)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Google returned an empty completion payload.");
        }

        return new AiTextCompletionResult(text);
    }

    private sealed record TextPart([property: JsonPropertyName("text")] string Text);

    private sealed record SystemInstruction([property: JsonPropertyName("parts")] IReadOnlyList<TextPart> Parts);

    private sealed record Content(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("parts")] IReadOnlyList<TextPart> Parts);

    private sealed record GenerationConfig(
        [property: JsonPropertyName("temperature")]
        double Temperature);

    private sealed record GenerateContentRequest(
        [property: JsonPropertyName("systemInstruction")]
        SystemInstruction SystemInstruction,
        [property: JsonPropertyName("contents")]
        IReadOnlyList<Content> Contents,
        [property: JsonPropertyName("generationConfig")]
        GenerationConfig GenerationConfig);

    private sealed record GenerateContentResponse(
        [property: JsonPropertyName("candidates")]
        IReadOnlyList<Candidate>? Candidates);

    private sealed record Candidate(
        [property: JsonPropertyName("content")]
        ContentPayload? Content);

    private sealed record ContentPayload(
        [property: JsonPropertyName("parts")] IReadOnlyList<TextPart>? Parts);
}