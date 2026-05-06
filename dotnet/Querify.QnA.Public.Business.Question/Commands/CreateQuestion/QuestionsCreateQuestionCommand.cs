using Querify.Models.QnA.Dtos.Question;
using MediatR;

namespace Querify.QnA.Public.Business.Question.Commands.CreateQuestion;

public sealed class QuestionsCreateQuestionCommand : IRequest<Guid>
{
    public required QuestionCreateRequestDto Request { get; set; }
}