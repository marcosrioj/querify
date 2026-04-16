using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Append-only operational journal entry for a question thread.
/// </summary>
public sealed class ThreadActivity : BaseEntity, IMustHaveTenant
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
    {
        ArgumentNullException.ThrowIfNull(question);
        Id = Guid.NewGuid();
        TenantId = DomainGuards.TenantIdOf(question, nameof(question));
        DomainGuards.InitializeAudit(this, createdBy, occurredAtUtc);

        if (answer is not null)
        {
            DomainGuards.EnsureSameTenant(question, answer, "thread activity question and answer");
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

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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

    /// <summary>
    /// Tenant boundary copied from the owning question thread.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Owning question identifier.
    /// </summary>
    public Guid QuestionId { get; private set; }

    /// <summary>
    /// Owning question navigation.
    /// </summary>
    public Question Question { get; private set; } = null!;

    /// <summary>
    /// Optional answer identifier targeted by the event.
    /// </summary>
    public Guid? AnswerId { get; private set; }

    /// <summary>
    /// Optional answer navigation targeted by the event.
    /// </summary>
    public Answer? Answer { get; private set; }

    /// <summary>
    /// Business event recorded for workflow, moderation, trust, and audit.
    /// </summary>
    public ActivityKind Kind { get; private set; }

    /// <summary>
    /// Identifies who or what caused the event.
    /// </summary>
    public ActorKind ActorKind { get; private set; } = ActorKind.System;

    /// <summary>
    /// Human-readable actor label stored alongside the event.
    /// </summary>
    public string? ActorLabel { get; private set; }

    /// <summary>
    /// Free-form notes about the event.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Flexible metadata payload for connector ids, moderation labels, vote payloads, or system details.
    /// </summary>
    public string? MetadataJson { get; private set; }

    /// <summary>
    /// Optional serialized snapshot used to preserve revision history without dedicated revision entities.
    /// </summary>
    public string? SnapshotJson { get; private set; }

    /// <summary>
    /// Revision pointer associated with the event when one exists.
    /// </summary>
    public int? RevisionNumber { get; private set; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

    public void Archive(string? archivedBy = null, DateTime? archivedAtUtc = null)
    {
        throw new InvalidOperationException("Thread activity is append-only and cannot be archived.");
    }
}
