using BaseFaq.AI.Business.Generation.Models;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.AI.Business.Generation.Service;

public sealed class ContentRefStudyService
{
    public ContentRefStudyResult Study(IReadOnlyCollection<(ContentRefKind Kind, string Locator)> refs)
    {
        var studiedRefs = new List<StudiedContentRef>();
        var skippedCount = 0;

        foreach (var contentRef in refs)
        {
            switch (contentRef.Kind)
            {
                case ContentRefKind.Web:
                    studiedRefs.Add(BuildStudied(contentRef, "webpage"));
                    break;
                case ContentRefKind.Pdf:
                    studiedRefs.Add(BuildStudied(contentRef, "pdf"));
                    break;
                case ContentRefKind.Document:
                    studiedRefs.Add(BuildStudied(contentRef, "document"));
                    break;
                case ContentRefKind.Video:
                    studiedRefs.Add(BuildStudied(contentRef, "video"));
                    break;
                default:
                    skippedCount++;
                    break;
            }
        }

        return new ContentRefStudyResult(
            refs.Count,
            studiedRefs.Count,
            skippedCount,
            studiedRefs);
    }

    private static StudiedContentRef BuildStudied((ContentRefKind Kind, string Locator) input, string sourceType)
    {
        var locator = input.Locator.Trim();

        return new StudiedContentRef(
            input.Kind,
            locator,
            $"Main subject inferred from {sourceType} locator: {locator}");
    }
}