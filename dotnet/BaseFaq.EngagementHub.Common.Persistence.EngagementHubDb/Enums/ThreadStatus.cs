namespace BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Enums;

/// <summary>
/// Defines the minimal lifecycle of an engagement thread.
/// </summary>
public enum ThreadStatus
{
    /// <summary>
    /// Thread is active and can continue receiving engagement items.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Thread has been completed and should be treated as historical until reopen behavior exists.
    /// </summary>
    Closed = 2
}
