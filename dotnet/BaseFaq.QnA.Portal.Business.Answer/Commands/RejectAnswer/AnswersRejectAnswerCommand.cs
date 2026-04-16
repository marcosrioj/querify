using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersRejectAnswerCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
