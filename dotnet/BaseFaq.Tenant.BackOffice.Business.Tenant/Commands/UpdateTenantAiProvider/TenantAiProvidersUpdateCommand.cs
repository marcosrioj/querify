using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantAiProvider;

public class TenantAiProvidersUpdateCommand : IRequest
{
    public required Guid Id { get; set; }
    public required Guid AiProviderId { get; set; }
    public required string AiProviderKey { get; set; }
}