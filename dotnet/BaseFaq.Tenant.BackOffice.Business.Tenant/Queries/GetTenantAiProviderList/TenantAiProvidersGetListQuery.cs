using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantAiProviderList;

public class TenantAiProvidersGetListQuery : IRequest<PagedResultDto<TenantAiProviderDto>>
{
    public required TenantAiProviderGetAllRequestDto Request { get; set; }
}