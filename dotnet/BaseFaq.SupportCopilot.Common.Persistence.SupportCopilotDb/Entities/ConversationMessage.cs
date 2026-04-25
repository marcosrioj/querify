using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Enums;

namespace BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Entities;

/// <summary>
/// Represents one message inside a Support Copilot conversation.
/// </summary>
public class ConversationMessage : BaseEntity, IMustHaveTenant
{
    public const int MaxBodyLength = 12000;

    public required Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    public required MessageActorKind ActorKind { get; set; }
    public required string Body { get; set; }
    public required DateTime SentAtUtc { get; set; }
    public required Guid TenantId { get; set; }
}
