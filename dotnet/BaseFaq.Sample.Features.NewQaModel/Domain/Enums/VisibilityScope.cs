namespace BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

/// <summary>
/// Defines who can see a space, question, or answer.
/// This is about audience exposure, not workflow state.
/// </summary>
public enum VisibilityScope
{
    /// <summary>
    /// Visible only inside internal operations.
    /// Useful for internal support, employee knowledge, or draft operational content.
    /// </summary>
    Internal = 1,

    /// <summary>
    /// Visible only to signed-in users.
    /// Useful when the content is customer-facing but should not be publicly open.
    /// </summary>
    Authenticated = 2,

    /// <summary>
    /// Publicly visible to any visitor, but not necessarily intended to be indexed.
    /// Useful for public widgets or pages with restricted search-surface ownership.
    /// </summary>
    Public = 3,

    /// <summary>
    /// Publicly visible and suitable for indexable, canonical search-facing pages.
    /// This is the strongest public exposure level.
    /// </summary>
    PublicIndexed = 4
}
