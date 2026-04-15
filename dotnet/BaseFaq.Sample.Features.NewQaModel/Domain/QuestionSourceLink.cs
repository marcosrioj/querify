using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class QuestionSourceLink : DomainEntity
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public Guid SourceId { get; set; }
    public KnowledgeSource Source { get; set; } = null!;

    /// <summary>
    /// Explains why the source is attached to the question.
    /// </summary>
    public SourceRole Role { get; set; } = SourceRole.QuestionOrigin;

    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? Excerpt { get; set; }

    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }
}
