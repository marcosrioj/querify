using BaseFaq.Models.Tenant.Dtos.TenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.DeleteTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.UpdateTenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Service;

public class TenantUserService(IMediator mediator) : ITenantUserService
{
    public Task<List<TenantUserDto>> GetAll(CancellationToken token)
    {
        return mediator.Send(new TenantUsersGetTenantUserListQuery(), token);
    }

    public Task<Guid> Create(TenantUserCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantUsersCreateTenantUserCommand
        {
            Email = requestDto.Email,
            Role = requestDto.Role
        }, token);
    }

    public async Task<Guid> Update(Guid id, TenantUserUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        await mediator.Send(new TenantUsersUpdateTenantUserCommand
        {
            Id = id,
            Role = requestDto.Role
        }, token);

        return id;
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new TenantUsersDeleteTenantUserCommand { Id = id }, token);
    }
}
