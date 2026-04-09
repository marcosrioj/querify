using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Feedback;
using MediatR;
using BaseFaq.Faq.Portal.Business.Feedback.Abstractions;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.CreateFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.DeleteFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.UpdateFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedbackList;

namespace BaseFaq.Faq.Portal.Business.Feedback.Service;

public class FeedbackService(IMediator mediator) : IFeedbackService
{
    public async Task<Guid> Create(FeedbackCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new FeedbacksCreateFeedbackCommand
        {
            Like = requestDto.Like,
            UnLikeReason = requestDto.UnLikeReason,
            FaqItemId = requestDto.FaqItemId
        };

        return await mediator.Send(command, token);
    }

    public Task<PagedResultDto<FeedbackDto>> GetAll(FeedbackGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new FeedbacksGetFeedbackListQuery { Request = requestDto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new FeedbacksDeleteFeedbackCommand { Id = id }, token);
    }

    public async Task<FeedbackDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new FeedbacksGetFeedbackQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"Feedback '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<Guid> Update(Guid id, FeedbackUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new FeedbacksUpdateFeedbackCommand
        {
            Id = id,
            Like = requestDto.Like,
            UnLikeReason = requestDto.UnLikeReason,
            FaqItemId = requestDto.FaqItemId
        };

        await mediator.Send(command, token);
        return id;
    }
}