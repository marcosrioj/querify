namespace BaseFaq.Models.Tenant.Dtos.Tenant;

public class TenantSetAiProviderCredentialsRequestDto
{
    public required Guid TenantId { get; set; }
    public required Guid AiProviderId { get; set; }
    public required string AiProviderKey { get; set; }
}
