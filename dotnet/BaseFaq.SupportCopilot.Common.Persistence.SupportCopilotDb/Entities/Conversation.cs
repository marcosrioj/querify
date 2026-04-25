using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Enums;

namespace BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Entities;

/// <summary>
/// Represents a support conversation owned by Support Copilot.
/// </summary>
public class Conversation : BaseEntity, IMustHaveTenant
{
    public const int MaxSubjectLength = 500;

    public required ConversationChannel Channel { get; set; }
    public required ConversationStatus Status { get; set; }
    public string? Subject { get; set; }
    public required DateTime StartedAtUtc { get; set; }
    public ICollection<ConversationMessage> Messages { get; set; } = [];
    public required Guid TenantId { get; set; }
}
