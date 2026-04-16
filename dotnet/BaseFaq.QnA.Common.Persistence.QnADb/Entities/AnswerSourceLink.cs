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
