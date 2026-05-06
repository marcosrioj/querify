using Querify.Models.QnA.Dtos.Question;
using Querify.QnA.Public.Business.Feedback.Abstractions;
using Querify.QnA.Public.Business.Feedback.Commands.CreateFeedback;
using MediatR;

namespace Querify.QnA.Public.Business.Feedback.Service;

public sealed class FeedbackService(IMediator mediator) : IFeedbackService
{
    public Task<Guid> Create(QuestionFeedbackCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new FeedbacksCreateFeedbackCommand { Request = dto }, token);
    }
}