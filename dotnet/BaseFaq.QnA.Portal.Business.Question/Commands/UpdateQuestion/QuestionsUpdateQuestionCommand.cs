using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;

public sealed class QuestionsUpdateQuestionCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
    public required QuestionUpdateRequestDto Request { get; set; }
}