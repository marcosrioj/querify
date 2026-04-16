using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Represents a topic used to classify questions and spaces.
/// </summary>
public class Topic : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 100;
    public const int MaxCategoryLength = 100;
    public const int MaxDescriptionLength = 500;

    /// <summary>
    /// Normalized topic name.
    /// </summary>
    public required string Name { get; set; } = null!;

    /// <summary>
    /// Optional category used to group related topics.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Additional topic description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Relationships between the topic and the spaces where it appears.
    /// </summary>
    public ICollection<QuestionSpaceTopic> Spaces { get; set; } = [];

    /// <summary>
    /// Relationships between the topic and the questions classified by it.
    /// </summary>
    public ICollection<QuestionTopic> Questions { get; set; } = [];

    /// <summary>
    /// Tenant that owns the topic.
    /// </summary>
    public required Guid TenantId { get; set; }
}
