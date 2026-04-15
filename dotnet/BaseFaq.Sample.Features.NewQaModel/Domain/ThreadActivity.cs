using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class ThreadActivity : DomainEntity
{
    public const int MaxActorLabelLength = 200;
    public const int MaxNotesLength = 4000;
    public const int MaxMetadataLength = 4000;
    public const int MaxSnapshotLength = 12000;

    private ThreadActivity(
        Question question,
        ActivityKind kind,
        ActorKind actorKind,
        string? actorLabel,
        string? notes,
        string? metadataJson,
        string? snapshotJson,
        int? revisionNumber,
        DateTime? occurredAtUtc,
        string? createdBy,
        Answer? answer = null)
        : base(DomainGuards.TenantIdOf(question, nameof(question)), createdBy, occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(question);

        if (answer is not null)
        {
            EnsureSameTenant(question, answer, "thread activity question and answer");
            DomainGuards.Ensure(answer.QuestionId == question.Id, "Thread activity answer must belong to the same question.");
        }

        QuestionId = question.Id;
        Question = question;
        AnswerId = answer?.Id;
        Answer = answer;
        Kind = kind;
        ActorKind = actorKind;
        ActorLabel = DomainGuards.Optional(actorLabel, MaxActorLabelLength, nameof(actorLabel));
        Notes = DomainGuards.Optional(notes, MaxNotesLength, nameof(notes));
        MetadataJson = DomainGuards.Json(metadataJson, MaxMetadataLength, nameof(metadataJson));
        SnapshotJson = DomainGuards.Json(snapshotJson, MaxSnapshotLength, nameof(snapshotJson));
        RevisionNumber = revisionNumber is null ? null : DomainGuards.NonNegative(revisionNumber.Value, nameof(revisionNumber));
        OccurredAtUtc = DomainGuards.Utc(occurredAtUtc ?? DateTime.UtcNow, nameof(occurredAtUtc));
    }

    private ThreadActivity()
    {
    }

    public static ThreadActivity ForQuestion(
        Question question,
        ActivityKind kind,
        ActorKind actorKind = ActorKind.System,
        string? actorLabel = null,
        string? notes = null,
        string? metadataJson = null,
        string? snapshotJson = null,
        int? revisionNumber = null,
        DateTime? occurredAtUtc = null,
        string? createdBy = null)
    {
        return new ThreadActivity(
            question,
            kind,
            actorKind,
            actorLabel,
            notes,
            metadataJson,
            snapshotJson,
            revisionNumber,
            occurredAtUtc,
            createdBy);
    }

    public static ThreadActivity ForAnswer(
        Question question,
        Answer answer,
        ActivityKind kind,
        ActorKind actorKind = ActorKind.System,
        string? actorLabel = null,
        string? notes = null,
        string? metadataJson = null,
        string? snapshotJson = null,
        int? revisionNumber = null,
        DateTime? occurredAtUtc = null,
        string? createdBy = null)
    {
        return new ThreadActivity(
            question,
            kind,
            actorKind,
            actorLabel,
            notes,
            metadataJson,
            snapshotJson,
            revisionNumber,
            occurredAtUtc,
            createdBy,
            answer);
    }

    public Guid QuestionId { get; private set; }
    public Question Question { get; private set; } = null!;

    public Guid? AnswerId { get; private set; }
    public Answer? Answer { get; private set; }

    /// <summary>
    /// Business event recorded for workflow, moderation, trust, and audit.
    /// </summary>
    public ActivityKind Kind { get; private set; }

    public ActorKind ActorKind { get; private set; } = ActorKind.System;
    public string? ActorLabel { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Flexible metadata payload for connector ids, moderation labels, vote payloads, or system details.
    /// </summary>
    public string? MetadataJson { get; private set; }

    /// <summary>
    /// Optional serialized snapshot used to preserve revision history without introducing dedicated revision entities.
    /// </summary>
    public string? SnapshotJson { get; private set; }

    public int? RevisionNumber { get; private set; }
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

    public override void Archive(string? archivedBy = null, DateTime? archivedAtUtc = null)
    {
        throw new InvalidOperationException("Thread activity is append-only and cannot be archived.");
    }
}
