using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Connects a question to a knowledge source, describing the role of that source
/// in the thread.
/// </summary>
public class QuestionSourceLink : BaseEntity, IMustHaveTenant
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;

    /// <summary>
    /// Id of the question linked to the source.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    /// Question linked to the source.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// Id of the source associated with the question.
    /// </summary>
    public required Guid SourceId { get; set; }

    /// <summary>
    /// Source associated with the question.
    /// </summary>
    public KnowledgeSource Source { get; set; } = null!;

    /// <summary>
    /// Role of the source for the question, such as origin, evidence, or reference.
    /// </summary>
    public SourceRole Role { get; set; } = SourceRole.QuestionOrigin;

    /// <summary>
    /// Human-readable label for the link.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Relevant source segment or scope for this link.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Literal or summarized excerpt from the source linked to the question.
    /// </summary>
    public string? Excerpt { get; set; }

    /// <summary>
    /// Display order or priority of the source in the set.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Confidence assigned to this specific link.
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Indicates whether this is the primary source for the question.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Tenant that owns the link.
    /// </summary>
    public required Guid TenantId { get; set; }
}
