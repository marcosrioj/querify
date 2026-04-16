using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RejectQuestion;

public sealed class QuestionsRejectQuestionCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
    public string? Notes { get; set; }
}