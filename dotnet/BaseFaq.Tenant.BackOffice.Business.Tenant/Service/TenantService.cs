using System.Net;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantList;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Service;

public class TenantService(IMediator mediator) : ITenantService
{
    public async Task<Guid> Create(TenantCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new TenantsCreateTenantCommand
        {
            Slug = requestDto.Slug,
            Name = requestDto.Name,
            Edition = requestDto.Edition,
            App = requestDto.App,
            ConnectionString = requestDto.ConnectionString,
            IsActive = requestDto.IsActive,
            UserId = requestDto.UserId
        };

        return await mediator.Send(command, token);
    }

    public Task<PagedResultDto<TenantDto>> GetAll(TenantGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantsGetTenantListQuery { Request = requestDto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new TenantsDeleteTenantCommand { Id = id }, token);
    }

    public async Task<TenantDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new TenantsGetTenantQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"Tenant '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<Guid> Update(Guid id, TenantUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new TenantsUpdateTenantCommand
        {
            Id = id,
            Slug = requestDto.Slug,
            Name = requestDto.Name,
            Edition = requestDto.Edition,
            ConnectionString = requestDto.ConnectionString,
            IsActive = requestDto.IsActive,
            UserId = requestDto.UserId
        };

        await mediator.Send(command, token);
        return id;
    }
}