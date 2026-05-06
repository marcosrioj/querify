namespace Querify.Models.QnA.Enums;

/// <summary>
/// Defines the lifecycle state of a Q&amp;A space.
/// </summary>
public enum SpaceStatus
{
    /// <summary>
    /// The space exists but is still being prepared.
    /// </summary>
    Draft = 1,

    /// <summary>
    /// The space is active and can participate in normal Q&amp;A flows.
    /// </summary>
    Active = 6,

    /// <summary>
    /// The space is retained for history but removed from normal operation.
    /// </summary>
    Archived = 11
}
