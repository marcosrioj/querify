using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.IsAiProviderKeyConfigured;

public class TenantsIsAiProviderKeyConfiguredQuery : IRequest<bool>
{
    public required Guid AiProviderId { get; set; }
}