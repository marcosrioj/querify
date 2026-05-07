namespace Querify.Models.QnA.Dtos.Source;

public class SourceUploadIntentResponseDto
{
    public required Guid SourceId { get; set; }
    public required string UploadUrl { get; set; }
    public required IReadOnlyDictionary<string, string> RequiredHeaders { get; set; }
    public required string StorageKey { get; set; }
    public required DateTime ExpiresAtUtc { get; set; }
}
