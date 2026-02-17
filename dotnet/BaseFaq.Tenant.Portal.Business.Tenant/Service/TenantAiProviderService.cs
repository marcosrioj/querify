using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetConfiguredAiProviders;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.IsAiProviderKeyConfigured;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Service;

public class TenantAiProviderService(IMediator mediator) : ITenantAiProviderService
{
    public Task<List<TenantAiProviderDto>> GetConfiguredAiProviders(CancellationToken token)
    {
        return mediator.Send(new TenantsGetConfiguredAiProvidersQuery(), token);
    }

    public Task<bool> IsAiProviderKeyConfigured(AiCommandType command, CancellationToken token)
    {
        return mediator.Send(new TenantsIsAiProviderKeyConfiguredQuery
        {
            Command = command
        }, token);
    }
}