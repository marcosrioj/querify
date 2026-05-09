namespace Querify.Models.QnA.Dtos.Source;

public class SourceUploadIntentRequestDto
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required long SizeBytes { get; set; }
    public required string Language { get; set; }
    public string? Label { get; set; }
    public string? ContextNote { get; set; }
    public string? ExternalId { get; set; }
    public string? MetadataJson { get; set; }
}
