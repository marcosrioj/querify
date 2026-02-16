using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetConfiguredAiProviders;

public class TenantsGetConfiguredAiProvidersQuery : IRequest<List<TenantAiProviderDto>>;