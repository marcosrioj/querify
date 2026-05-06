using Querify.Models.Tenant.Dtos.TenantUser;
using Querify.Tenant.BackOffice.Business.Tenant.Abstractions;
using Querify.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantUser;
using Querify.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantUser;
using Querify.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantUser;
using Querify.Tenant.BackOffice.Business.Tenant.Queries.GetTenantUserList;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Service;

public class TenantUserService(IMediator mediator) : ITenantUserService
{
    public Task<List<TenantUserDto>> GetAll(Guid tenantId, CancellationToken token)
    {
        return mediator.Send(new TenantUsersGetTenantUserListQuery { TenantId = tenantId }, token);
    }

    public Task<Guid> Create(Guid tenantId, TenantUserCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantUsersCreateTenantUserCommand
        {
            TenantId = tenantId,
            Name = requestDto.Name,
            Email = requestDto.Email,
            Role = requestDto.Role
        }, token);
    }

    public async Task<Guid> Update(Guid tenantId, Guid id, TenantUserUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        await mediator.Send(new TenantUsersUpdateTenantUserCommand
        {
            TenantId = tenantId,
            Id = id,
            Role = requestDto.Role
        }, token);

        return id;
    }

    public Task Delete(Guid tenantId, Guid id, CancellationToken token)
    {
        return mediator.Send(new TenantUsersDeleteTenantUserCommand
        {
            TenantId = tenantId,
            Id = id
        }, token);
    }
}
