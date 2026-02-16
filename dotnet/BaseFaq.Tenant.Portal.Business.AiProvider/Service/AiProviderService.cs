using BaseFaq.Models.Tenant.Dtos.AiProvider;
using BaseFaq.Tenant.Portal.Business.AiProvider.Abstractions;
using BaseFaq.Tenant.Portal.Business.AiProvider.Queries.GetAiProviderList;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.AiProvider.Service;

public class AiProviderService(IMediator mediator) : IAiProviderService
{
    public Task<List<AiProviderDto>> GetAll(CancellationToken token)
    {
        return mediator.Send(new AiProvidersGetAiProviderListQuery(), token);
    }
}