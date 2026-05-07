namespace Querify.Models.QnA.Dtos.Source;

public class SourceDownloadUrlDto
{
    public required string Url { get; set; }
    public required DateTime ExpiresAtUtc { get; set; }
}
