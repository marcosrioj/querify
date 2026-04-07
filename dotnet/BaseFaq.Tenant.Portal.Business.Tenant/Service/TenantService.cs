using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.RefreshAllowedTenantCache;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.SetAiProviderCredentials;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetClientKey;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Service;

public class TenantService(IMediator mediator) : ITenantService
{
    public Task<List<TenantSummaryDto>> GetAll(CancellationToken token)
    {
        return mediator.Send(new TenantsGetAllTenantsQuery(), token);
    }

    public Task<bool> CreateOrUpdate(TenantCreateOrUpdateRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new TenantsCreateOrUpdateTenantsCommand
        {
            TenantId = requestDto.TenantId,
            Name = requestDto.Name,
            Edition = requestDto.Edition
        };

        return mediator.Send(command, token);
    }

    public Task<bool> RefreshAllowedTenantCache(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantsRefreshAllowedTenantCacheCommand { TenantId = tenantId }, token);
    }

    public Task<string?> GetClientKey(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantsGetClientKeyQuery { TenantId = tenantId }, token);
    }

    public Task<string> GenerateNewClientKey(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantsGenerateNewClientKeyCommand { TenantId = tenantId }, token);
    }

    public Task<bool> SetAiProviderCredentials(TenantSetAiProviderCredentialsRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantsSetAiProviderCredentialsCommand
        {
            TenantId = requestDto.TenantId,
            AiProviderId = requestDto.AiProviderId,
            AiProviderKey = requestDto.AiProviderKey
        }, token);
    }
}
