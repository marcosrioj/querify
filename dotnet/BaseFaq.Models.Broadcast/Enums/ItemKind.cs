namespace BaseFaq.Models.Broadcast.Enums;

/// <summary>
/// Classifies a captured Broadcast item.
/// </summary>
public enum ItemKind
{
    /// <summary>
    /// Top-level published item that can anchor a public interaction thread.
    /// </summary>
    Post = 1,

    /// <summary>
    /// Public reply or nested contribution within a Broadcast thread.
    /// </summary>
    Comment = 6,

    /// <summary>
    /// Message captured from a shared Broadcast thread where responses can be seen by many people.
    /// </summary>
    SharedMessage = 11,

    /// <summary>
    /// Captured item is known by Broadcast but not represented by a more specific item kind yet.
    /// </summary>
    Other = 16
}
