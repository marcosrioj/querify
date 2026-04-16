using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class QuestionSourceLink : BaseEntity, IMustHaveTenant
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;
    public required Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public required Guid SourceId { get; set; }
    public KnowledgeSource Source { get; set; } = null!;
    public SourceRole Role { get; set; } = SourceRole.QuestionOrigin;
    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? Excerpt { get; set; }
    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }

    public required Guid TenantId { get; set; }
}