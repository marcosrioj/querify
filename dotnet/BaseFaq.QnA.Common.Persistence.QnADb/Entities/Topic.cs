using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class Topic : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 100;
    public const int MaxCategoryLength = 100;
    public const int MaxDescriptionLength = 500;

    public required Guid TenantId { get; set; }
    public required string Name { get; set; } = null!;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public ICollection<QuestionSpaceTopic> QuestionSpaceTopics { get; set; } = [];
    public ICollection<QuestionTopic> QuestionTopics { get; set; } = [];
}
