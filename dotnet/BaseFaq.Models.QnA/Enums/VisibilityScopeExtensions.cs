namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Helper methods for reasoning about Q&amp;A visibility rules.
/// </summary>
public static class VisibilityScopeExtensions
{
    /// <summary>
    /// Returns true when the visibility can be exposed to any public visitor.
    /// </summary>
    public static bool IsPubliclyVisible(this VisibilityScope visibility) =>
        visibility is VisibilityScope.Public;
}
