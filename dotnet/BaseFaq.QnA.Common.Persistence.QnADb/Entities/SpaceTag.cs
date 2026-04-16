using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Link entity between a space and a tag.
/// </summary>
public class SpaceTag : BaseEntity, IMustHaveTenant
{
    /// <summary>
    /// Id of the classified space.
    /// </summary>
    public required Guid SpaceId { get; set; }

    /// <summary>
    /// Space classified by the tag.
    /// </summary>
    public Space Space { get; set; } = null!;

    /// <summary>
    /// Id of the tag applied to the space.
    /// </summary>
    public required Guid TagId { get; set; }

    /// <summary>
    /// Tag applied to the space.
    /// </summary>
    public Tag Tag { get; set; } = null!;

    /// <summary>
    /// Tenant that owns the relationship.
    /// </summary>
    public required Guid TenantId { get; set; }
}
