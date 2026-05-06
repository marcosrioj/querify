using Querify.Models.Tenant.Dtos.Tenant;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using Querify.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;
using Querify.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;
using Querify.Tenant.Portal.Business.Tenant.Commands.RefreshAllowedTenantCache;
using Querify.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;
using Querify.Tenant.Portal.Business.Tenant.Queries.GetClientKey;
using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Service;

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

    public Task<bool> RefreshAllowedTenantCache(CancellationToken token)
    {
        return mediator.Send(new TenantsRefreshAllowedTenantCacheCommand(), token);
    }

    public Task<string?> GetClientKey(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantsGetClientKeyQuery { TenantId = tenantId }, token);
    }

    public Task<string> GenerateNewClientKey(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantsGenerateNewClientKeyCommand { TenantId = tenantId }, token);
    }
}
