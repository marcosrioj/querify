using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Question.Commands;

public sealed class QuestionsCreateQuestionCommand : IRequest<Guid>
{
    public required QuestionCreateRequestDto Request { get; set; }
}
