namespace BaseFaq.Models.Tenant.Dtos.TenantAiProvider;

public class TenantAiProviderCreateRequestDto
{
    public required Guid TenantId { get; set; }
    public required Guid AiProviderId { get; set; }
    public required string AiProviderKey { get; set; }
}