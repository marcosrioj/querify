namespace BaseFaq.Models.Broadcast.Enums;

/// <summary>
/// Describes the broad Broadcast channel family.
/// </summary>
public enum ChannelKind
{
    /// <summary>
    /// Thread came from a social network or social publishing surface.
    /// </summary>
    Social = 1,

    /// <summary>
    /// Thread came from a community forum, group, or public discussion space.
    /// </summary>
    Community = 2,

    /// <summary>
    /// Thread came from a shared messaging surface such as a group, channel, or community space.
    /// </summary>
    SharedMessaging = 3,

    /// <summary>
    /// Thread source is known by Broadcast but not represented by a more specific channel family yet.
    /// </summary>
    Other = 99
}
