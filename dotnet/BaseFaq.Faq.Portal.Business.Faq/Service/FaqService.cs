using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Faq;
using MediatR;
using BaseFaq.Faq.Portal.Business.Faq.Abstractions;
using BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaq;
using BaseFaq.Faq.Portal.Business.Faq.Commands.DeleteFaq;
using BaseFaq.Faq.Portal.Business.Faq.Commands.RequestGeneration;
using BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaq;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaq;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqList;

namespace BaseFaq.Faq.Portal.Business.Faq.Service;

public class FaqService(IMediator mediator) : IFaqService
{
    public async Task<Guid> Create(FaqCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new FaqsCreateFaqCommand
        {
            Name = requestDto.Name,
            Language = requestDto.Language,
            Status = requestDto.Status
        };

        return await mediator.Send(command, token);
    }

    public Task<PagedResultDto<FaqDto>> GetAll(FaqGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new FaqsGetFaqListQuery { Request = requestDto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new FaqsDeleteFaqCommand { Id = id }, token);
    }

    public async Task<Guid> RequestGeneration(Guid faqId, CancellationToken token)
    {
        return await mediator.Send(new FaqsRequestGenerationCommand { FaqId = faqId }, token);
    }

    public async Task<FaqDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new FaqsGetFaqQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"FAQ '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<Guid> Update(Guid id, FaqUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new FaqsUpdateFaqCommand
        {
            Id = id,
            Name = requestDto.Name,
            Language = requestDto.Language,
            Status = requestDto.Status
        };

        await mediator.Send(command, token);
        return id;
    }
}
