using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Link entity between a question and a tag.
/// </summary>
public class QuestionTag : BaseEntity, IMustHaveTenant
{
    /// <summary>
    /// Id of the classified question.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    /// Question classified by the tag.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// Id of the tag applied to the question.
    /// </summary>
    public required Guid TagId { get; set; }

    /// <summary>
    /// Tag applied to the question.
    /// </summary>
    public Tag Tag { get; set; } = null!;

    /// <summary>
    /// Tenant that owns the relationship.
    /// </summary>
    public required Guid TenantId { get; set; }
}
