using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.RejectAnswer;

public sealed class AnswersRejectAnswerCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}