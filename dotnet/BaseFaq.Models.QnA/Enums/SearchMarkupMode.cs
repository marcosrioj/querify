namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines how the surface should behave from a search markup perspective.
/// </summary>
public enum SearchMarkupMode
{
    /// <summary>
    /// The surface behaves mainly like a curated list or collection page.
    /// Best for grouped knowledge pages and editorial question collections.
    /// </summary>
    CuratedList = 1,

    /// <summary>
    /// The surface behaves mainly like a single-question page.
    /// Best for canonical question threads with accepted or validated answers.
    /// </summary>
    QuestionPage = 2,

    /// <summary>
    /// The surface supports both collection and single-question behaviors.
    /// This is useful when a product needs both indexable lists and canonical thread pages.
    /// </summary>
    Hybrid = 3,

    /// <summary>
    /// Search-specific markup is intentionally disabled.
    /// Useful when another renderer owns search markup or when indexing is not desired.
    /// </summary>
    Off = 4
}
