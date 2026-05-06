using Querify.Models.QnA.Dtos.Question;
using MediatR;

namespace Querify.QnA.Portal.Business.Question.Commands.UpdateQuestion;

public sealed class QuestionsUpdateQuestionCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
    public required QuestionUpdateRequestDto Request { get; set; }
}