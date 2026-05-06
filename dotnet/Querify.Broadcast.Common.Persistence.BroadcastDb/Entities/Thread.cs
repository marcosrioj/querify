using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.Broadcast.Enums;

namespace Querify.Broadcast.Common.Persistence.BroadcastDb.Entities;

/// <summary>
/// Represents a public or community interaction thread captured by Broadcast.
/// </summary>
public class Thread : BaseEntity, IMustHaveTenant
{
    public const int MaxTitleLength = 1000;

    /// <summary>
    /// Broad public interaction channel family used to route thread behavior without coupling to a specific provider.
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
    /// Broadcast items owned by this thread; tenant integrity is enforced through the parent relationship.
    /// </summary>
    public ICollection<Item> Items { get; set; } = [];

    /// <summary>
    /// Tenant that owns the thread and scopes tenant filters and relationship validation.
    /// </summary>
    public required Guid TenantId { get; set; }
}
