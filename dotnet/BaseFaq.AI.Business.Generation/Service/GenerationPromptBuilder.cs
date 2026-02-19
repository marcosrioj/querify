using System.Text;
using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Generation.Abstractions;
using BaseFaq.AI.Business.Generation.Models;
using BaseFaq.Models.Ai.Contracts.Generation;

namespace BaseFaq.AI.Business.Generation.Service;

public sealed class GenerationPromptBuilder : IGenerationPromptBuilder
{
    public GenerationPromptData BuildPromptData(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs,
        AiProviderContext providerContext)
    {
        return new GenerationPromptData(
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
}