using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
///     Records business and audit events in the lifecycle of a question
///     and, optionally, of an answer.
/// </summary>
public class Activity : BaseEntity, IMustHaveTenant
{
    public const int MaxActorLabelLength = 200;
    public const int MaxUserPrintLength = 200;
    public const int MaxIpLength = 100;
    public const int MaxUserAgentLength = 1000;
    public const int MaxNotesLength = 4000;
    public const int MaxMetadataLength = 4000;

    /// <summary>
    ///     Id of the question that owns the event.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    ///     Question that owns the event.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    ///     Id of the answer related to the event, when present.
    /// </summary>
    public Guid? AnswerId { get; set; }

    /// <summary>
    ///     Answer related to the event, when present.
    /// </summary>
    public Answer? Answer { get; set; }

    /// <summary>
    ///     Type of event recorded for the question.
    /// </summary>
    public required ActivityKind Kind { get; set; }

    /// <summary>
    ///     Type of actor that executed or originated the event.
    /// </summary>
    public required ActorKind ActorKind { get; set; }

    /// <summary>
    ///     Name or label of the actor associated with the event.
    /// </summary>
    public string? ActorLabel { get; set; }

    /// <summary>
    ///     Calculated canonical identity of the effective actor for public and authenticated activity flows.
    /// </summary>
    public required string UserPrint { get; set; }

    /// <summary>
    ///     Calculated originating IP address for the activity.
    /// </summary>
    public required string Ip { get; set; }

    /// <summary>
    ///     Calculated originating user agent for the activity.
    /// </summary>
    public required string UserAgent { get; set; }

    /// <summary>
    ///     Free-form notes about the event.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     Serialized metadata for the event.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    ///     Timestamp when the event occurred.
    /// </summary>
    public required DateTime OccurredAtUtc { get; set; }

    /// <summary>
    ///     Tenant that owns the event.
    /// </summary>
    public required Guid TenantId { get; set; }
}
