using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Feedback.Commands.CreateFeedback;

public sealed class FeedbacksCreateFeedbackCommand : IRequest<Guid>
{
    public required QuestionFeedbackCreateRequestDto Request { get; set; }
}