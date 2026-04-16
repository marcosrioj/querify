using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Represents a tag used to classify questions and spaces.
/// </summary>
public class Tag : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 100;

    /// <summary>
    /// Normalized tag name.
    /// </summary>
    public required string Name { get; set; } = null!;

    /// <summary>
    /// Relationships between the tag and the spaces where it appears.
    /// </summary>
    public ICollection<SpaceTag> Spaces { get; set; } = [];

    /// <summary>
    /// Relationships between the tag and the questions classified by it.
    /// </summary>
    public ICollection<QuestionTag> Questions { get; set; } = [];

    /// <summary>
    /// Tenant that owns the tag.
    /// </summary>
    public required Guid TenantId { get; set; }
}
