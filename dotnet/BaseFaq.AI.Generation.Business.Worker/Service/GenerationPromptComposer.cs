using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.AI.Common.Providers.Models;
using BaseFaq.AI.Generation.Business.Worker.Abstractions;
using System.Text;

namespace BaseFaq.AI.Generation.Business.Worker.Service;

public sealed class GenerationPromptComposer : IGenerationPromptComposer
{
    private const string Domain = "generation";
    private const string PromptVersion = "2026-02-15.generation.v1";

    public GenerationPromptComposition Compose(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        AiProviderCredential providerCredential)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(studiedRefs);
        ArgumentNullException.ThrowIfNull(providerCredential);

        var provider = NormalizeProvider(providerCredential.Provider);

        return new GenerationPromptComposition(
            Domain,
            PromptVersion,
            provider,
            BuildSystemPrompt(provider),
            BuildUserPrompt(request, studiedRefs),
            BuildExpectedJsonSchema());
    }

    private static string BuildSystemPrompt(string provider)
    {
        var providerDirective = provider switch
        {
            "openai" => "Prefer concise structured outputs and avoid unnecessary prose.",
            "anthropic" => "Be explicit about uncertainty and avoid fabricated facts.",
            "google" => "Optimize for grounded factual synthesis and high precision.",
            _ => "Produce grounded, deterministic output with strict schema adherence."
        };

        return string.Join(Environment.NewLine, [
            "You are a multilingual FAQ generation engine.",
            "Use only supplied source context. Do not invent citations or facts.",
            "Write clear customer-facing language while preserving technical accuracy.",
            "If context is insufficient, state the missing data in uncertaintyNotes.",
            providerDirective
        ]);
    }

    private static string BuildUserPrompt(FaqGenerationRequestedV1 request, ContentRefStudyResult studiedRefs)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Task: generate a FAQ draft from studied references.");
        builder.AppendLine($"faqId: {request.FaqId:D}");
        builder.AppendLine($"tenantId: {request.TenantId:D}");
        builder.AppendLine($"language: {request.Language}");
        builder.AppendLine($"refs.total: {studiedRefs.TotalCount}");
        builder.AppendLine($"refs.processed: {studiedRefs.ProcessedCount}");
        builder.AppendLine($"refs.skipped: {studiedRefs.SkippedCount}");
        builder.AppendLine("references:");

        foreach (var studiedRef in studiedRefs.StudiedRefs)
        {
            builder.AppendLine(
                $"- kind={studiedRef.Kind}, locator={studiedRef.Locator}, inferredSubject={studiedRef.MainSubject}");
        }

        builder.AppendLine("Output must follow the provided JSON schema exactly.");
        return builder.ToString();
    }

    private static string BuildExpectedJsonSchema()
    {
        return """
               {
                 "type": "object",
                 "required": ["question", "summary", "answer", "confidence", "citations", "uncertaintyNotes"],
                 "properties": {
                   "question": { "type": "string", "maxLength": 1000 },
                   "summary": { "type": "string", "maxLength": 250 },
                   "answer": { "type": "string", "maxLength": 5000 },
                   "confidence": { "type": "integer", "minimum": 0, "maximum": 100 },
                   "citations": {
                     "type": "array",
                     "items": { "type": "string", "maxLength": 2000 }
                   },
                   "uncertaintyNotes": {
                     "type": "array",
                     "items": { "type": "string", "maxLength": 500 }
                   }
                 }
               }
               """;
    }

    private static string NormalizeProvider(string? provider)
    {
        return string.IsNullOrWhiteSpace(provider)
            ? "unknown"
            : provider.Trim().ToLowerInvariant();
    }
}