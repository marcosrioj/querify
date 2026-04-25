namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Enums;

/// <summary>
/// Describes the broad engagement channel family.
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
    /// Thread came from a direct or private messaging surface.
    /// </summary>
    Messaging = 3,

    /// <summary>
    /// Thread source is known by Engagement Hub but not represented by a more specific channel family yet.
    /// </summary>
    Other = 99
}
