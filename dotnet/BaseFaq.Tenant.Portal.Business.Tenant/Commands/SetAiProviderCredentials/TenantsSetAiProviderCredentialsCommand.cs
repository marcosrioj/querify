using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.SetAiProviderCredentials;

public sealed class TenantsSetAiProviderCredentialsCommand : IRequest<bool>
{
    public required Guid TenantId { get; set; }
    public required Guid AiProviderId { get; set; }
    public required string AiProviderKey { get; set; }
}
