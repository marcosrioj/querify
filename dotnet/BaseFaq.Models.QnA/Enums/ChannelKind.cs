namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Describes the Answer Hub intake path through which reusable knowledge entered the system.
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
    /// Brought in through a bulk import or sync process.
    /// </summary>
    Import = 5,

    /// <summary>
    /// Fallback for uncommon or not-yet-classified channels.
    /// </summary>
    Other = 99
}
