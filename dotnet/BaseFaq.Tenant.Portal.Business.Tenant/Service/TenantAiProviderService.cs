using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetConfiguredAiProviders;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.IsAiProviderKeyConfigured;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Service;

public class TenantAiProviderService(IMediator mediator) : ITenantAiProviderService
{
    public Task<List<TenantAiProviderDto>> GetConfiguredAiProviders(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantsGetConfiguredAiProvidersQuery { TenantId = tenantId }, token);
    }

    public Task<bool> IsAiProviderKeyConfigured(Guid tenantId, AiCommandType command, CancellationToken token)
    {
        return mediator.Send(new TenantsIsAiProviderKeyConfiguredQuery
        {
            TenantId = tenantId,
            Command = command
        }, token);
    }
}
