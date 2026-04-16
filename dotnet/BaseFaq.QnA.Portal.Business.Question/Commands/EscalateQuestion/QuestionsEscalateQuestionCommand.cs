using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.EscalateQuestion;

public sealed class QuestionsEscalateQuestionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string? Notes { get; set; }
}