namespace Querify.Models.QnA.Dtos.Source;

public class SourceCreateRequestDto
{
    public required string Locator { get; set; }
    public string? Label { get; set; }
    public string? ContextNote { get; set; }
    public string? ExternalId { get; set; }
    public required string Language { get; set; }
    public string? MediaType { get; set; }
    public string? MetadataJson { get; set; }
}
