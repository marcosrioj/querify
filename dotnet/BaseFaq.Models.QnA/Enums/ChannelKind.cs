namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Describes the channel through which a question, vote, or feedback signal entered the system.
/// </summary>
public enum ChannelKind
{
    /// <summary>
    /// Created directly by an operator or internal user.
    /// </summary>
    Manual = 1,

    /// <summary>
    /// Captured from an embed or widget experience.
    /// </summary>
    Widget = 2,

    /// <summary>
    /// Submitted through an external or internal API.
    /// </summary>
    Api = 3,

    /// <summary>
    /// Imported from a help center surface.
    /// </summary>
    HelpCenter = 4,

    /// <summary>
    /// Imported or derived from a support ticket flow.
    /// </summary>
    Ticket = 5,

    /// <summary>
    /// Originated from a community forum or discussion area.
    /// </summary>
    Community = 6,

    /// <summary>
    /// Originated from a social platform such as comments or direct engagement threads.
    /// </summary>
    Social = 7,

    /// <summary>
    /// Originated from a chat conversation such as live chat or messaging support.
    /// </summary>
    Chat = 8,

    /// <summary>
    /// Brought in through a bulk import or sync process.
    /// </summary>
    Import = 9,

    /// <summary>
    /// Fallback for uncommon or not-yet-classified channels.
    /// </summary>
    Other = 99
}
