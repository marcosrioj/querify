namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines the operating mode of a Q&amp;A space.
/// </summary>
public enum SpaceKind
{
    /// <summary>
    /// Answers are controlled by the tenant and exposed after internal approval.
    /// </summary>
    ControlledPublication = 1,

    /// <summary>
    /// Participants may contribute, but publication is moderated.
    /// </summary>
    ModeratedCollaboration = 2,

    /// <summary>
    /// Public participation, votes, accepted answers, or decisions are visible and auditable.
    /// </summary>
    PublicValidation = 3
}
