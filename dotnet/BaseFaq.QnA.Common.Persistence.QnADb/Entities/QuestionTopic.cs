using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class QuestionTopic : BaseEntity, IMustHaveTenant
{
    public required Guid TenantId { get; set; }
    public required Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public required Guid TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
}
