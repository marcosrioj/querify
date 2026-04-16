using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Link entity between a question and a topic.
/// </summary>
public class QuestionTopic : BaseEntity, IMustHaveTenant
{
    /// <summary>
    /// Id of the classified question.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    /// Question classified by the topic.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// Id of the topic applied to the question.
    /// </summary>
    public required Guid TopicId { get; set; }

    /// <summary>
    /// Topic applied to the question.
    /// </summary>
    public Topic Topic { get; set; } = null!;

    /// <summary>
    /// Tenant that owns the relationship.
    /// </summary>
    public required Guid TenantId { get; set; }
}
