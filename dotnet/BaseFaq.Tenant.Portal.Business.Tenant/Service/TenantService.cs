using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;
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
            Name = requestDto.Name,
            Edition = requestDto.Edition
        };

        return mediator.Send(command, token);
    }

    public Task<string?> GetClientKey(CancellationToken token)
    {
        return mediator.Send(new TenantsGetClientKeyQuery(), token);
    }

    public Task<string> GenerateNewClientKey(CancellationToken token)
    {
        return mediator.Send(new TenantsGenerateNewClientKeyCommand(), token);
    }

    public Task SetAiProviderCredentials(TenantSetAiProviderCredentialsRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantsSetAiProviderCredentialsCommand
        {
            AiProviderId = requestDto.AiProviderId,
            AiProviderKey = requestDto.AiProviderKey
        }, token);
    }
}