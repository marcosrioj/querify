namespace Querify.Models.QnA.Enums;

/// <summary>
/// Controls how tags are produced during source-to-space generation.
/// </summary>
public enum SourceGenerationTagMode
{
    /// <summary>
    /// Do not generate or attach tags.
    /// </summary>
    None = 1,

    /// <summary>
    /// Suggest tags in run metadata without writing tag relationships.
    /// </summary>
    SuggestOnly = 6,

    /// <summary>
    /// Create or reuse tags and attach them to generated content.
    /// </summary>
    CreateAndAttach = 11
}
