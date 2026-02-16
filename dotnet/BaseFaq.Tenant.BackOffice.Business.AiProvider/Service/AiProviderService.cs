using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.AiProvider;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.CreateAiProvider;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.DeleteAiProvider;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.UpdateAiProvider;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Queries.GetAiProvider;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Queries.GetAiProviderList;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Service;

public class AiProviderService(IMediator mediator) : IAiProviderService
{
    public async Task<Guid> Create(AiProviderCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new AiProvidersCreateAiProviderCommand
        {
            Provider = requestDto.Provider,
            Model = requestDto.Model,
            Prompt = requestDto.Prompt,
            Command = requestDto.Command
        };

        return await mediator.Send(command, token);
    }

    public async Task<Guid> Update(Guid id, AiProviderUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new AiProvidersUpdateAiProviderCommand
        {
            Id = id,
            Provider = requestDto.Provider,
            Model = requestDto.Model,
            Prompt = requestDto.Prompt,
            Command = requestDto.Command
        };

        await mediator.Send(command, token);
        return id;
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new AiProvidersDeleteAiProviderCommand { Id = id }, token);
    }

    public async Task<AiProviderDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new AiProvidersGetAiProviderQuery { Id = id }, token);

        if (result is null)
        {
            throw new ApiErrorException(
                $"AI Provider '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public Task<PagedResultDto<AiProviderDto>> GetAll(AiProviderGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new AiProvidersGetAiProviderListQuery { Request = requestDto }, token);
    }
}