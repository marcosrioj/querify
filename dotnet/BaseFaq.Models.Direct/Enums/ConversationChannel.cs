namespace BaseFaq.Models.Direct.Enums;

/// <summary>
/// Describes where a support conversation started.
/// </summary>
public enum ConversationChannel
{
    /// <summary>
    /// Conversation started from an embedded web chat surface.
    /// </summary>
    WebChat = 1,

    /// <summary>
    /// Conversation started inside an authenticated application experience.
    /// </summary>
    InApp = 2,

    /// <summary>
    /// Conversation originated from email and may represent an asynchronous support thread.
    /// </summary>
    Email = 3,

    /// <summary>
    /// Conversation source is known by Direct but not represented by a more specific channel yet.
    /// </summary>
    Other = 99
}
