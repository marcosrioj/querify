using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands;

public sealed class QuestionsUpdateQuestionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required QuestionUpdateRequestDto Request { get; set; }
}
