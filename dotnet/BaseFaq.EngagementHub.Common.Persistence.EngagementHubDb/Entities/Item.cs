using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Enums;

namespace BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Entities;

/// <summary>
/// Represents one captured item inside an Engagement Hub thread.
/// </summary>
public class Item : BaseEntity, IMustHaveTenant
{
    public const int MaxBodyLength = 12000;

    public required Guid ThreadId { get; set; }
    public Thread Thread { get; set; } = null!;
    public required ItemKind Kind { get; set; }
    public required ActorKind ActorKind { get; set; }
    public required string Body { get; set; }
    public required DateTime CapturedAtUtc { get; set; }
    public required Guid TenantId { get; set; }
}
