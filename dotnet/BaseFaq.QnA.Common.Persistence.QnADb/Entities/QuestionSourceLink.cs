using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
///     Connects a question to a source, describing the role of that source
///     for the question.
/// </summary>
public class QuestionSourceLink : BaseEntity, IMustHaveTenant
{
    /// <summary>
    ///     Id of the question linked to the source.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    ///     Question linked to the source.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    ///     Id of the source associated with the question.
    /// </summary>
    public required Guid SourceId { get; set; }

    /// <summary>
    ///     Source associated with the question.
    /// </summary>
    public Source Source { get; set; } = null!;

    /// <summary>
    ///     Role of the source for the question, such as origin, evidence, or reference.
    /// </summary>
    public required SourceRole Role { get; set; }

    /// <summary>
    ///     Display order or priority of the source in the set.
    /// </summary>
    public required int Order { get; set; }

    /// <summary>
    ///     Tenant that owns the link.
    /// </summary>
    public required Guid TenantId { get; set; }
}
