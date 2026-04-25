using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Enums;

namespace BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Entities;

/// <summary>
/// Represents an engagement thread captured by Engagement Hub.
/// </summary>
public class Thread : BaseEntity, IMustHaveTenant
{
    public const int MaxTitleLength = 1000;

    public required ChannelKind Channel { get; set; }
    public required ThreadStatus Status { get; set; }
    public string? Title { get; set; }
    public required DateTime CapturedAtUtc { get; set; }
    public ICollection<Item> Items { get; set; } = [];
    public required Guid TenantId { get; set; }
}
