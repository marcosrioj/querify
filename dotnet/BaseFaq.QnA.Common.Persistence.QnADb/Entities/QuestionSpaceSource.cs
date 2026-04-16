using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Link entity between a space and a curated source.
/// </summary>
public class QuestionSpaceSource : BaseEntity, IMustHaveTenant
{
    /// <summary>
    /// Id of the space that exposes the source.
    /// </summary>
    public required Guid QuestionSpaceId { get; set; }

    /// <summary>
    /// Space that exposes the source.
    /// </summary>
    public QuestionSpace QuestionSpace { get; set; } = null!;

    /// <summary>
    /// Id of the curated source associated with the space.
    /// </summary>
    public required Guid KnowledgeSourceId { get; set; }

    /// <summary>
    /// Curated source associated with the space.
    /// </summary>
    public KnowledgeSource KnowledgeSource { get; set; } = null!;

    /// <summary>
    /// Tenant that owns the relationship.
    /// </summary>
    public required Guid TenantId { get; set; }
}
