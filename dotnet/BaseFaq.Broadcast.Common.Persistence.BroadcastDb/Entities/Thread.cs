using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Enums;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities;

/// <summary>
/// Represents an engagement thread captured by Engagement Hub.
/// </summary>
public class Thread : BaseEntity, IMustHaveTenant
{
    public const int MaxTitleLength = 1000;

    /// <summary>
    /// Broad engagement channel family used to route thread behavior without coupling to a specific provider.
    /// </summary>
    public required ChannelKind Channel { get; set; }

    /// <summary>
    /// Current lifecycle state used to decide whether the thread remains active or has been completed.
    /// </summary>
    public required ThreadStatus Status { get; set; }

    /// <summary>
    /// Optional topic or provider title used for display and lookup when the source channel supplies one.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Engagement items owned by this thread; tenant integrity is enforced through the parent relationship.
    /// </summary>
    public ICollection<Item> Items { get; set; } = [];

    /// <summary>
    /// Tenant that owns the thread and scopes tenant filters and relationship validation.
    /// </summary>
    public required Guid TenantId { get; set; }
}
