using Querify.Models.QnA.Dtos.Question;
using MediatR;

namespace Querify.QnA.Public.Business.Feedback.Commands.CreateFeedback;

public sealed class FeedbacksCreateFeedbackCommand : IRequest<Guid>
{
    public required QuestionFeedbackCreateRequestDto Request { get; set; }
}