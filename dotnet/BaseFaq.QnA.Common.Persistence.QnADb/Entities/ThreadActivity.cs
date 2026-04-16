using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class ThreadActivity : BaseEntity, IMustHaveTenant
{
    public const int MaxActorLabelLength = 200;
    public const int MaxNotesLength = 4000;
    public const int MaxMetadataLength = 4000;
    public const int MaxSnapshotLength = 12000;

    public required Guid TenantId { get; set; }
    public required Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public Guid? AnswerId { get; set; }
    public Answer? Answer { get; set; }
    public ActivityKind Kind { get; set; }
    public ActorKind ActorKind { get; set; } = ActorKind.System;
    public string? ActorLabel { get; set; }
    public string? Notes { get; set; }
    public string? MetadataJson { get; set; }
    public string? SnapshotJson { get; set; }
    public int? RevisionNumber { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
