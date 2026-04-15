namespace BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

/// <summary>
///     Defines how submissions are moderated before or after they become visible.
/// </summary>
public enum ModerationPolicy
{
    /// <summary>
    ///     No explicit moderation gate.
    ///     Content can flow directly into the visible experience.
    /// </summary>
    None = 0,

    /// <summary>
    ///     New submissions must be reviewed before they become visible.
    ///     This is the safest default for external or AI-generated input.
    /// </summary>
    PreModeration = 1,

    /// <summary>
    ///     New submissions become visible first and are reviewed afterward.
    ///     This works better for high-velocity communities with lighter risk tolerance.
    /// </summary>
    PostModeration = 2,

    /// <summary>
    ///     Trusted contributors can publish more directly, while less trusted actors still require review.
    ///     This supports progressive governance without fully opening the system.
    /// </summary>
    TrustedContributors = 3
}