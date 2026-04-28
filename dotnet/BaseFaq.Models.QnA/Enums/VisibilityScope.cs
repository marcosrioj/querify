namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines who can see a space, question, answer, or source.
/// This is about audience exposure, not workflow state.
/// </summary>
public enum VisibilityScope
{
    /// <summary>
    /// Visible only to authenticated tenant users or authenticated product users.
    /// This is the non-public exposure level for operational and gated content.
    /// </summary>
    Authenticated = 1,

    /// <summary>
    /// Publicly visible to any visitor.
    /// </summary>
    Public = 2
}
