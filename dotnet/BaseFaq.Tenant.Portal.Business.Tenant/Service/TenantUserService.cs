using BaseFaq.Models.Tenant.Dtos.TenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.DeleteTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.UpsertTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Service;

public class TenantUserService(IMediator mediator) : ITenantUserService
{
    public Task<List<TenantUserDto>> GetAll(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantUsersGetTenantUserListQuery { TenantId = tenantId }, token);
    }

    public Task<Guid> Upsert(TenantUserCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantUsersUpsertTenantUserCommand
        {
            TenantId = requestDto.TenantId,
            Name = requestDto.Name,
            Email = requestDto.Email,
            Role = requestDto.Role
        }, token);
    }

    public Task Delete(Guid tenantId, Guid id, CancellationToken token)
    {
        return mediator.Send(new TenantUsersDeleteTenantUserCommand { TenantId = tenantId, Id = id }, token);
    }
}
