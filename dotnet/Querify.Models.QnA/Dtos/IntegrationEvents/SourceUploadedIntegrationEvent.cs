namespace Querify.Models.QnA.Dtos.IntegrationEvents;

public class SourceUploadedIntegrationEvent
{
    public required Guid TenantId { get; set; }
    public required Guid SourceId { get; set; }
    public required string StorageKey { get; set; }
    public string? ClientChecksum { get; set; }
    public required DateTime UploadedAtUtc { get; set; }
}
