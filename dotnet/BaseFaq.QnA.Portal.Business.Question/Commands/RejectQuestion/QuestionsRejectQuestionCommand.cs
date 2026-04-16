using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RejectQuestion;

public sealed class QuestionsRejectQuestionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string? Notes { get; set; }
}
