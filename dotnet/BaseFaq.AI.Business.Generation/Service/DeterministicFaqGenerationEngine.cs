using System.Text;
using BaseFaq.AI.Business.Common.Models;
using BaseFaq.Models.Ai.Contracts.Generation;

namespace BaseFaq.AI.Business.Generation.Service;

public sealed class DeterministicFaqGenerationEngine : IFaqGenerationEngine
{
    private const string PromptDomain = "generation";
    private const string PromptVersion = "2026-02-19.generation.v2";

    public GeneratedFaqDraft Generate(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        AiProviderContext providerContext)
    {
        var promptData = BuildPromptData(request, studiedRefs, providerContext);

        return new GeneratedFaqDraft(
            BuildDraftQuestion(studiedRefs),
            BuildDraftSummary(request.FaqId, studiedRefs),
            BuildDraftContent(request.FaqId, studiedRefs),
            ConfidenceFromStudy(studiedRefs),
            promptData);
    }

    private static int ConfidenceFromStudy(ContentRefStudyResult studyResult)
    {
        if (studyResult.TotalCount == 0)
        {
            return 0;
        }

        var ratio = studyResult.ProcessedCount / (double)studyResult.TotalCount;
        return (int)Math.Round(Math.Clamp(ratio, 0d, 1d) * 100, MidpointRounding.AwayFromZero);
    }

    private static string BuildDraftQuestion(ContentRefStudyResult studyResult)
    {
        if (studyResult.ProcessedCount == 0)
        {
            return "Generated draft question based on available content references";
        }

        var grouped = studyResult.StudiedRefs
            .GroupBy(x => x.Kind)
            .Select(x => x.Key.ToString())
            .ToArray();

        return $"Generated draft question based on: {string.Join(", ", grouped)}";
    }

    private static string BuildDraftSummary(Guid faqId, ContentRefStudyResult studyResult)
    {
        return
            $"Draft summary for FAQ {faqId}. ContentRefs total={studyResult.TotalCount}, processed={studyResult.ProcessedCount}, skipped={studyResult.SkippedCount}.";
    }

    private static string BuildDraftContent(Guid faqId, ContentRefStudyResult studyResult)
    {
        if (studyResult.ProcessedCount == 0)
        {
            return
                $"Generated draft placeholder for FAQ {faqId}. No processable ContentRef kind was found (all were skipped by business rules).";
        }

        var lines = studyResult.StudiedRefs
            .Select(x => $"{x.Kind} ({x.Locator}): {x.MainSubject}");

        return
            $"Generated draft placeholder for FAQ {faqId}. Source study:{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
    }

    private static GenerationPromptData BuildPromptData(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        AiProviderContext providerContext)
    {
        var provider = NormalizeProvider(providerContext.Provider);

        return new GenerationPromptData(
            PromptDomain,
            PromptVersion,
            provider,
            ResolvePromptTemplate(providerContext.Prompt),
            BuildPromptInput(request, studiedRefs),
            BuildOutputSchema());
    }

    private static string ResolvePromptTemplate(string? prompt)
    {
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return prompt;
        }

        return
            "You are a multilingual FAQ generation engine. Use only supplied context and return schema-compliant JSON.";
    }

    private static string BuildPromptInput(FaqGenerationRequestedV1 request, ContentRefStudyResult studiedRefs)
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

    private static string BuildOutputSchema()
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
