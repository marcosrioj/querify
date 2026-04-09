using BaseFaq.Faq.Public.Business.Feedback.Abstractions;
using BaseFaq.Faq.Public.Business.Feedback.Commands.CreateFeedback;
using MediatR;
using BaseFaq.Models.Faq.Dtos.Feedback;

namespace BaseFaq.Faq.Public.Business.Feedback.Service;

public class FeedbackService(IMediator mediator) : IFeedbackService
{
    public async Task<Guid> Feedback(FeedbackCreateRequestDto requestDto, CancellationToken token)
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
}