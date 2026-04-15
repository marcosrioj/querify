using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class AnswerSourceLink : DomainEntity
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;

    public Guid AnswerId { get; set; }
    public Answer Answer { get; set; } = null!;

    public Guid SourceId { get; set; }
    public KnowledgeSource Source { get; set; } = null!;

    /// <summary>
    /// Explains why the source is attached to the answer.
    /// </summary>
    public SourceRole Role { get; set; } = SourceRole.Evidence;

    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? Excerpt { get; set; }

    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }
}
