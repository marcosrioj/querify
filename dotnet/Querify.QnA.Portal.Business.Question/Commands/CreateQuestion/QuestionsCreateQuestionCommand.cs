using Querify.Models.QnA.Dtos.Question;
using MediatR;

namespace Querify.QnA.Portal.Business.Question.Commands.CreateQuestion;

public sealed class QuestionsCreateQuestionCommand : IRequest<Guid>
{
    public required QuestionCreateRequestDto Request { get; set; }
}