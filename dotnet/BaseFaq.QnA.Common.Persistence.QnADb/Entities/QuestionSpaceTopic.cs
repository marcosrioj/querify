using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Link entity between a space and a topic.
/// </summary>
public class QuestionSpaceTopic : BaseEntity, IMustHaveTenant
{
    /// <summary>
    /// Id of the classified space.
    /// </summary>
    public required Guid QuestionSpaceId { get; set; }

    /// <summary>
    /// Space classified by the topic.
    /// </summary>
    public QuestionSpace QuestionSpace { get; set; } = null!;

    /// <summary>
    /// Id of the topic applied to the space.
    /// </summary>
    public required Guid TopicId { get; set; }

    /// <summary>
    /// Topic applied to the space.
    /// </summary>
    public Topic Topic { get; set; } = null!;

    /// <summary>
    /// Tenant that owns the relationship.
    /// </summary>
    public required Guid TenantId { get; set; }
}
