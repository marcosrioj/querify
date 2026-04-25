using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Broadcast.Enums;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities;

/// <summary>
/// Represents one captured item inside a Broadcast thread.
/// </summary>
public class Item : BaseEntity, IMustHaveTenant
{
    public const int MaxBodyLength = 12000;

    /// <summary>
    /// Parent thread that owns the item; it must reference a thread from the same tenant.
    /// </summary>
    public required Guid ThreadId { get; set; }

    /// <summary>
    /// Navigation to the owning thread used for persistence relationship tracking and tenant validation.
    /// </summary>
    public Thread Thread { get; set; } = null!;

    /// <summary>
    /// Captured item shape used to distinguish posts, comments, messages, and fallback public interaction entries.
    /// </summary>
    public required ItemKind Kind { get; set; }

    /// <summary>
    /// Author role used to separate external audience activity, brand responses, and system entries.
    /// </summary>
    public required ActorKind ActorKind { get; set; }

    /// <summary>
    /// Text content captured for the Broadcast timeline.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Time the item was captured from the Broadcast channel; it can differ from the record creation time.
    /// </summary>
    public required DateTime CapturedAtUtc { get; set; }

    /// <summary>
    /// Tenant that owns the item and must match the owning thread tenant.
    /// </summary>
    public required Guid TenantId { get; set; }
}
