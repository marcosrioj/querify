namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Enums;

/// <summary>
/// Classifies a captured engagement item.
/// </summary>
public enum ItemKind
{
    /// <summary>
    /// Top-level published item that can anchor a public engagement thread.
    /// </summary>
    Post = 1,

    /// <summary>
    /// Public reply or nested contribution within an engagement thread.
    /// </summary>
    Comment = 2,

    /// <summary>
    /// Direct or private message captured as part of an engagement thread.
    /// </summary>
    Message = 3,

    /// <summary>
    /// Captured item is known by Engagement Hub but not represented by a more specific item kind yet.
    /// </summary>
    Other = 99
}
