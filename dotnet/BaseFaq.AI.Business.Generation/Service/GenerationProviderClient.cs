using System.Text.Json;
using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Models;
using BaseFaq.AI.Business.Generation.Abstractions;
using BaseFaq.AI.Business.Generation.Models;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.AI.Business.Generation.Service;

public sealed class GenerationProviderClient(
    IAiProviderRuntimeContextResolver runtimeContextResolver,
    IAiTextCompletionGateway textCompletionGateway)
    : IGenerationProviderClient
{
    public async Task<GeneratedFaqDraft> GenerateDraftAsync(
        AiProviderContext providerContext,
        GenerationPromptData promptData,
        CancellationToken cancellationToken)
    {
        var runtimeContext = runtimeContextResolver.Resolve(providerContext, AiCommandType.Generation);

        var prompt = new AiTextCompletionRequest(
            promptData.Template,
            $"{promptData.Input}{Environment.NewLine}{Environment.NewLine}Return JSON only with this schema:{Environment.NewLine}{promptData.OutputSchema}");

        var completion = await textCompletionGateway.CompleteAsync(runtimeContext, prompt, cancellationToken);
        return ParseGeneratedDraft(completion.Content);
    }

    private static GeneratedFaqDraft ParseGeneratedDraft(string content)
    {
        var raw = JsonPayloadReader.ExtractJsonObject(content);

        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;

        var question = TryReadString(root, "question");
        var summary = TryReadString(root, "summary");
        var answer = TryReadString(root, "answer");
        var confidence = ReadConfidence(root);

        if (string.IsNullOrWhiteSpace(question) ||
            string.IsNullOrWhiteSpace(summary) ||
            string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException("Generation provider response is missing required fields.");
        }

        return new GeneratedFaqDraft(question, summary, answer, confidence);
    }

    private static string? TryReadString(JsonElement root, string propertyName)
    {
        if (TryGetPropertyIgnoreCase(root, propertyName, out var value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static int ReadConfidence(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "confidence", out var confidenceElement))
        {
            return 80;
        }

        return confidenceElement.ValueKind switch
        {
            JsonValueKind.Number when confidenceElement.TryGetInt32(out var integerValue)
                => Math.Clamp(integerValue, 0, 100),
            JsonValueKind.Number when confidenceElement.TryGetDouble(out var doubleValue)
                => Math.Clamp((int)Math.Round(doubleValue), 0, 100),
            _ => 80
        };
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }
}