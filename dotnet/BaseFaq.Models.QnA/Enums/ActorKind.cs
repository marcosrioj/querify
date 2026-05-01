namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Identifies who or what caused a activity event.
/// </summary>
public enum ActorKind
{
    /// <summary>
    /// The platform itself or an automated workflow caused the event.
    /// </summary>
    System = 1,

    /// <summary>
    /// An end customer or external user caused the event.
    /// </summary>
    Customer = 6,

    /// <summary>
    /// A general contributor caused the event.
    /// This may be an internal or community participant.
    /// </summary>
    Contributor = 11,

    /// <summary>
    /// A moderator or editor caused the event.
    /// </summary>
    Moderator = 16,

    /// <summary>
    /// An external integration or connector caused the event.
    /// </summary>
    Integration = 21
}
