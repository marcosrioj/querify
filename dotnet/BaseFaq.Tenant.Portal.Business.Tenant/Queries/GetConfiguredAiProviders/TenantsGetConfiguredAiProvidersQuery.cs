using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetConfiguredAiProviders;

public sealed class TenantsGetConfiguredAiProvidersQuery : IRequest<List<TenantAiProviderDto>>
{
    public required Guid TenantId { get; set; }
}
