using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class KnowledgeSource : BaseEntity, IMustHaveTenant
{
    public const int MaxLocatorLength = 1000;
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxSystemNameLength = 100;
    public const int MaxExternalIdLength = 250;
    public const int MaxLanguageLength = 50;
    public const int MaxMediaTypeLength = 100;
    public const int MaxChecksumLength = 128;
    public const int MaxMetadataLength = 8000;

    public required Guid TenantId { get; set; }
    public SourceKind Kind { get; set; } = SourceKind.Other;
    public required string Locator { get; set; } = null!;
    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? SystemName { get; set; }
    public string? ExternalId { get; set; }
    public string? Language { get; set; }
    public string? MediaType { get; set; }
    public string? Checksum { get; set; }
    public string? MetadataJson { get; set; }
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public bool AllowsPublicCitation { get; set; }
    public bool AllowsPublicExcerpt { get; set; }
    public bool IsAuthoritative { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
    public DateTime? LastVerifiedAtUtc { get; set; }
    public ICollection<QuestionSpaceSource> QuestionSpaceSources { get; set; } = [];
    public ICollection<QuestionSourceLink> Questions { get; set; } = [];
    public ICollection<AnswerSourceLink> Answers { get; set; } = [];
}
