using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Source;

public class SourceDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Locator { get; set; }
    public string? StorageKey { get; set; }
    public string? Label { get; set; }
    public string? ContextNote { get; set; }
    public string? ExternalId { get; set; }
    public required string Language { get; set; }
    public string? MediaType { get; set; }
    public long? SizeBytes { get; set; }
    public required string Checksum { get; set; }
    public string? MetadataJson { get; set; }
    public required SourceUploadStatus UploadStatus { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public DateTime? LastUpdatedAtUtc { get; set; }
    public int SpaceUsageCount { get; set; }
    public int QuestionUsageCount { get; set; }
    public int AnswerUsageCount { get; set; }
}
