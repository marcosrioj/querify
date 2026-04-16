using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Records business and audit events in the lifecycle of a question
/// and, optionally, of an answer.
/// </summary>
public class ThreadActivity : BaseEntity, IMustHaveTenant
{
    public const int MaxActorLabelLength = 200;
    public const int MaxUserPrintLength = 200;
    public const int MaxNotesLength = 4000;
    public const int MaxMetadataLength = 4000;
    /// <summary>
    /// Id of the question that owns the event.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    /// Question that owns the event.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// Id of the answer related to the event, when present.
    /// </summary>
    public Guid? AnswerId { get; set; }

    /// <summary>
    /// Answer related to the event, when present.
    /// </summary>
    public Answer? Answer { get; set; }

    /// <summary>
    /// Type of event recorded in the thread.
    /// </summary>
    public ActivityKind Kind { get; set; }

    /// <summary>
    /// Type of actor that executed or originated the event.
    /// </summary>
    public ActorKind ActorKind { get; set; } = ActorKind.System;

    /// <summary>
    /// Name or label of the actor associated with the event.
    /// </summary>
    public string? ActorLabel { get; set; }

    /// <summary>
    /// Canonical identity of the effective actor for public and authenticated activity flows.
    /// </summary>
    public string? UserPrint { get; set; }

    /// <summary>
    /// Free-form notes about the event.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Serialized metadata for the event.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tenant that owns the event.
    /// </summary>
    public required Guid TenantId { get; set; }
}
