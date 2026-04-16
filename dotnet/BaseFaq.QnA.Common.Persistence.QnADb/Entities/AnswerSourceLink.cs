using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Connects an answer to a knowledge source used as evidence,
/// citation, or reference.
/// </summary>
public class AnswerSourceLink : BaseEntity, IMustHaveTenant
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;

    /// <summary>
    /// Id of the answer linked to the source.
    /// </summary>
    public required Guid AnswerId { get; set; }

    /// <summary>
    /// Answer linked to the source.
    /// </summary>
    public Answer Answer { get; set; } = null!;

    /// <summary>
    /// Id of the source associated with the answer.
    /// </summary>
    public required Guid SourceId { get; set; }

    /// <summary>
    /// Source associated with the answer.
    /// </summary>
    public KnowledgeSource Source { get; set; } = null!;

    /// <summary>
    /// Role of the source for the answer, such as evidence or canonical reference.
    /// </summary>
    public SourceRole Role { get; set; } = SourceRole.Evidence;

    /// <summary>
    /// Human-readable label for the link.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Relevant source segment or scope for this link.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Literal or summarized excerpt from the source linked to the answer.
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
    /// Indicates whether this is the primary source for the answer.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Tenant that owns the link.
    /// </summary>
    public required Guid TenantId { get; set; }
}
