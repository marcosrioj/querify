using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Direct.Enums;

namespace BaseFaq.Direct.Common.Persistence.DirectDb.Entities;

/// <summary>
/// Represents a 1:1 support conversation owned by Direct.
/// </summary>
public class Conversation : BaseEntity, IMustHaveTenant
{
    public const int MaxSubjectLength = 500;

    /// <summary>
    /// Entry surface used to route support conversation behavior without storing QnA channel state.
    /// </summary>
    public required ConversationChannel Channel { get; set; }

    /// <summary>
    /// Current lifecycle state used to decide whether the conversation is still active or already completed.
    /// </summary>
    public required ConversationStatus Status { get; set; }

    /// <summary>
    /// Optional human-readable topic supplied by the support channel; it is not required to identify the conversation.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Messages owned by this conversation; tenant integrity is enforced through the parent relationship.
    /// </summary>
    public ICollection<ConversationMessage> Messages { get; set; } = [];

    /// <summary>
    /// Tenant that owns the conversation and scopes tenant filters and relationship validation.
    /// </summary>
    public required Guid TenantId { get; set; }
}
