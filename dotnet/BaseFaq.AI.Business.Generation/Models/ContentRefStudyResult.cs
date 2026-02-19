namespace BaseFaq.AI.Business.Generation.Models;

public sealed record ContentRefStudyResult(
    int TotalCount,
    int ProcessedCount,
    int SkippedCount,
    IReadOnlyList<StudiedContentRef> StudiedRefs);