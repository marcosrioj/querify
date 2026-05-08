namespace Querify.Models.QnA.Dtos.Source;

public class SourceExternalUrlInspectionDto
{
    public required bool IsReachable { get; set; }
    public int? Status { get; set; }
    public string? StatusText { get; set; }
    public string? FinalUrl { get; set; }
    public string? ContentType { get; set; }
    public long? ContentLengthBytes { get; set; }
    public string? LastModified { get; set; }
}
