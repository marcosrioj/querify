using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Public.Business.Feedback.Abstractions;
using BaseFaq.QnA.Public.Business.Feedback.Commands;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Feedback.Service;

public sealed class FeedbackService(IMediator mediator) : IFeedbackService
{
    public Task<Guid> Create(QuestionFeedbackCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new FeedbacksCreateFeedbackCommand { Request = dto }, token);
    }
}
