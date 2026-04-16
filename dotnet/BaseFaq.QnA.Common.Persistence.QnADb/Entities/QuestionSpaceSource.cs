using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class QuestionSpaceSource : BaseEntity, IMustHaveTenant
{
    public required Guid TenantId { get; set; }
    public required Guid QuestionSpaceId { get; set; }
    public QuestionSpace QuestionSpace { get; set; } = null!;
    public required Guid KnowledgeSourceId { get; set; }
    public KnowledgeSource KnowledgeSource { get; set; } = null!;
}
