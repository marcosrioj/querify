using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.QnA.Common.Domain.Entities;

/// <summary>
///     Link entity between a space and a curated source.
/// </summary>
public class SpaceSource : BaseEntity, IMustHaveTenant
{
    /// <summary>
    ///     Id of the space that exposes the source.
    /// </summary>
    public required Guid SpaceId { get; set; }

    /// <summary>
    ///     Space that exposes the source.
    /// </summary>
    public Space Space { get; set; } = null!;

    /// <summary>
    ///     Id of the curated source associated with the space.
    /// </summary>
    public required Guid SourceId { get; set; }

    /// <summary>
    ///     Curated source associated with the space.
    /// </summary>
    public Source Source { get; set; } = null!;

    /// <summary>
    ///     Tenant that owns the relationship.
    /// </summary>
    public required Guid TenantId { get; set; }
}