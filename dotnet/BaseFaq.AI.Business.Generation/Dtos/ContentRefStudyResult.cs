namespace BaseFaq.AI.Business.Generation.Dtos;

public sealed record ContentRefStudyResult(
    int TotalCount,
    int ProcessedCount,
    int SkippedCount,
    IReadOnlyList<StudiedContentRef> StudiedRefs);