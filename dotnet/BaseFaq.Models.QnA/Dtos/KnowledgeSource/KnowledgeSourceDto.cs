using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.KnowledgeSource;

public class KnowledgeSourceDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public SourceKind Kind { get; set; }
    public string Locator { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? SystemName { get; set; }
    public string? ExternalId { get; set; }
    public string? Language { get; set; }
    public string? MediaType { get; set; }
    public string? Checksum { get; set; }
    public string? MetadataJson { get; set; }
    public VisibilityScope Visibility { get; set; }
    public bool AllowsPublicCitation { get; set; }
    public bool AllowsPublicExcerpt { get; set; }
    public bool IsAuthoritative { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
    public DateTime? LastVerifiedAtUtc { get; set; }
}
