using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.AiProvider;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Queries.GetAiProviderList;

public sealed class AiProvidersGetAiProviderListQuery : IRequest<PagedResultDto<AiProviderDto>>
{
    public required AiProviderGetAllRequestDto Request { get; set; }
}