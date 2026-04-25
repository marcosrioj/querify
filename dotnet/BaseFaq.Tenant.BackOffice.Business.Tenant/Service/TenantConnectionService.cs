using System.Net;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.TenantConnection;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantConnection;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantConnection;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantConnection;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnection;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnectionList;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Service;

public class TenantConnectionService(IMediator mediator) : ITenantConnectionService
{
    public async Task<Guid> Create(TenantConnectionCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new TenantConnectionsCreateTenantConnectionCommand
        {
            Module = requestDto.Module,
            ConnectionString = requestDto.ConnectionString,
            IsCurrent = requestDto.IsCurrent
        };

        return await mediator.Send(command, token);
    }

    public Task<PagedResultDto<TenantConnectionDto>> GetAll(TenantConnectionGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantConnectionsGetTenantConnectionListQuery { Request = requestDto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new TenantConnectionsDeleteTenantConnectionCommand { Id = id }, token);
    }

    public async Task<TenantConnectionDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new TenantConnectionsGetTenantConnectionQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"Tenant connection '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<Guid> Update(Guid id, TenantConnectionUpdateRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new TenantConnectionsUpdateTenantConnectionCommand
        {
            Id = id,
            Module = requestDto.Module,
            ConnectionString = requestDto.ConnectionString,
            IsCurrent = requestDto.IsCurrent
        };

        await mediator.Send(command, token);
        return id;
    }
}