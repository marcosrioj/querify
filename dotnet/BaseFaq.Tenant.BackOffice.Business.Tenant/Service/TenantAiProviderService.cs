using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantAiProvider;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantAiProvider;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantAiProvider;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantAiProvider;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantAiProviderList;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Service;

public class TenantAiProviderService(IMediator mediator) : ITenantAiProviderService
{
    public Task<Guid> Create(TenantAiProviderCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantAiProvidersCreateCommand
        {
            TenantId = requestDto.TenantId,
            AiProviderId = requestDto.AiProviderId,
            AiProviderKey = requestDto.AiProviderKey
        }, token);
    }

    public async Task<Guid> Update(Guid id, TenantAiProviderUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        await mediator.Send(new TenantAiProvidersUpdateCommand
        {
            Id = id,
            AiProviderId = requestDto.AiProviderId,
            AiProviderKey = requestDto.AiProviderKey
        }, token);

        return id;
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new TenantAiProvidersDeleteCommand { Id = id }, token);
    }

    public async Task<TenantAiProviderDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new TenantAiProvidersGetQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException($"Tenant AI Provider '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public Task<PagedResultDto<TenantAiProviderDto>> GetAll(TenantAiProviderGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new TenantAiProvidersGetListQuery { Request = requestDto }, token);
    }
}