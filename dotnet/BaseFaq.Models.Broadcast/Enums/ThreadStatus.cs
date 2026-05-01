namespace BaseFaq.Models.Broadcast.Enums;

/// <summary>
/// Defines the minimal lifecycle of a Broadcast thread.
/// </summary>
public enum ThreadStatus
{
    /// <summary>
    /// Thread is active and can continue receiving Broadcast items.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Thread has been completed and should be treated as historical until reopen behavior exists.
    /// </summary>
    Closed = 6
}
