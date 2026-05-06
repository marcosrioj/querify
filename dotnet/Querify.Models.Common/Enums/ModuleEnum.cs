namespace Querify.Models.Common.Enums;

/// <summary>
/// Identifies the Querify module that owns a tenant, request context, or module-specific data boundary.
/// </summary>
public enum ModuleEnum
{
    /// <summary>
    /// Owns control-plane metadata such as tenants, users, billing, entitlements, and module connection mapping.
    /// </summary>
    Tenant = 1,

    /// <summary>
    /// Owns approved knowledge, questions, answers, sources, tags, public signals, and QnA workflow state.
    /// </summary>
    QnA = 6,

    /// <summary>
    /// Owns direct 1:1 resolution conversations, messages, handoff context, and agent-assist records.
    /// </summary>
    Direct = 11,

    /// <summary>
    /// Owns public and community interaction capture, response coordination, and social signal records.
    /// </summary>
    Broadcast = 16,

    /// <summary>
    /// Owns validation, governance, decision history, and auditability records.
    /// </summary>
    Trust = 21
}
