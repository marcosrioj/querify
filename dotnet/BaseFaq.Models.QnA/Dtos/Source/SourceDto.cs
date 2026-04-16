using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Source;

public class SourceDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required SourceKind Kind { get; set; }
    public required string Locator { get; set; }
    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? SystemName { get; set; }
    public string? ExternalId { get; set; }
    public string? Language { get; set; }
    public string? MediaType { get; set; }
    public string? Checksum { get; set; }
    public string? MetadataJson { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required bool AllowsPublicCitation { get; set; }
    public required bool AllowsPublicExcerpt { get; set; }
    public required bool IsAuthoritative { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
    public DateTime? LastVerifiedAtUtc { get; set; }
}
