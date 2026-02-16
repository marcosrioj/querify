using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantAiProvider;

public class TenantAiProvidersCreateCommand : IRequest<Guid>
{
    public required Guid TenantId { get; set; }
    public required Guid AiProviderId { get; set; }
    public required string AiProviderKey { get; set; }
}