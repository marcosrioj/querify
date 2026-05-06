using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.Direct.Enums;

namespace Querify.Direct.Common.Persistence.DirectDb.Entities;

/// <summary>
/// Represents one message inside a Direct conversation.
/// </summary>
public class ConversationMessage : BaseEntity, IMustHaveTenant
{
    public const int MaxBodyLength = 12000;

    /// <summary>
    /// Parent conversation that owns the message; it must reference a conversation from the same tenant.
    /// </summary>
    public required Guid ConversationId { get; set; }

    /// <summary>
    /// Navigation to the owning conversation used for persistence relationship tracking and tenant validation.
    /// </summary>
    public Conversation Conversation { get; set; } = null!;

    /// <summary>
    /// Author role used to separate user input, automated Direct output, agent replies, and system entries.
    /// </summary>
    public required MessageActorKind ActorKind { get; set; }

    /// <summary>
    /// Text content captured for the conversation timeline.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Time the message was sent in the support channel; it can differ from the record creation time.
    /// </summary>
    public required DateTime SentAtUtc { get; set; }

    /// <summary>
    /// Tenant that owns the message and must match the owning conversation tenant.
    /// </summary>
    public required Guid TenantId { get; set; }
}
