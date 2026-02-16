using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.SetAiProviderCredentials;

public class TenantsSetAiProviderCredentialsCommand : IRequest
{
    public required Guid AiProviderId { get; set; }
    public required string AiProviderKey { get; set; }
}