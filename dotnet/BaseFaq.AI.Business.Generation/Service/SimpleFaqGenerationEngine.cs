using BaseFaq.AI.Business.Generation.Abstractions;
using BaseFaq.AI.Business.Generation.Dtos;
using BaseFaq.Models.Ai.Contracts.Generation;

namespace BaseFaq.AI.Business.Generation.Service;

public sealed class SimpleFaqGenerationEngine : IFaqGenerationEngine
{
    public GeneratedFaqDraft Generate(
        FaqGenerationRequestedV1 request,
        ContentRefStudyResult studiedRefs)
    {
        return new GeneratedFaqDraft(
            BuildDraftQuestion(studiedRefs),
            BuildDraftSummary(request.FaqId, studiedRefs),
            BuildDraftContent(request.FaqId, studiedRefs),
            ConfidenceFromStudy(studiedRefs));
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
}