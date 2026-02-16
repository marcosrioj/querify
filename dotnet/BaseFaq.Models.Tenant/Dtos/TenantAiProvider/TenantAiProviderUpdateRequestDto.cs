namespace BaseFaq.Models.Tenant.Dtos.TenantAiProvider;

public class TenantAiProviderUpdateRequestDto
{
    public required Guid AiProviderId { get; set; }
    public required string AiProviderKey { get; set; }
}