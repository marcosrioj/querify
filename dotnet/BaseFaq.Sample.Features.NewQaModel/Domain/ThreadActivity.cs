using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class ThreadActivity : DomainEntity
{
    public const int MaxActorLabelLength = 200;
    public const int MaxNotesLength = 4000;
    public const int MaxMetadataLength = 4000;
    public const int MaxSnapshotLength = 12000;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public Guid? AnswerId { get; set; }
    public Answer? Answer { get; set; }

    /// <summary>
    /// Business event recorded for workflow, moderation, trust, and audit.
    /// </summary>
    public ActivityKind Kind { get; set; }

    public ActorKind ActorKind { get; set; } = ActorKind.System;
    public string? ActorLabel { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Flexible metadata payload for connector ids, moderation labels, vote payloads, or system details.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Optional serialized snapshot used to preserve revision history without introducing dedicated revision entities.
    /// </summary>
    public string? SnapshotJson { get; set; }

    public int? RevisionNumber { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
