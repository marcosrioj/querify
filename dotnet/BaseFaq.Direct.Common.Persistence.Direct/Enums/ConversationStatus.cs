namespace BaseFaq.Direct.Common.Persistence.DirectDb.Enums;

/// <summary>
/// Defines the minimal lifecycle of a support conversation.
/// </summary>
public enum ConversationStatus
{
    /// <summary>
    /// Conversation is active and can continue receiving support messages.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Conversation has been completed and should be treated as historical until reopen behavior exists.
    /// </summary>
    Closed = 2
}
