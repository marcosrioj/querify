using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class QuestionSpaceTopic : BaseEntity, IMustHaveTenant
{
    public required Guid QuestionSpaceId { get; set; }
    public QuestionSpace QuestionSpace { get; set; } = null!;
    public required Guid TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
    public required Guid TenantId { get; set; }
}