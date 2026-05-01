namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines who can see a space, question, answer, or source.
/// This is about audience exposure, not workflow state.
/// </summary>
public enum VisibilityScope
{
    /// <summary>
    /// Visible only inside the tenant portal.
    /// This is the default exposure level for operational content.
    /// </summary>
    Internal = 1,

    /// <summary>
    /// Visible outside the portal only to authenticated users.
    /// This is for gated external customer or product experiences.
    /// </summary>
    Authenticated = 6,

    /// <summary>
    /// Visible outside the portal to any visitor.
    /// </summary>
    Public = 11
}
