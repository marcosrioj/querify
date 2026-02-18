using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantAiProvider;

public sealed class TenantAiProvidersGetQuery : IRequest<TenantAiProviderDto?>
{
    public required Guid Id { get; set; }
}