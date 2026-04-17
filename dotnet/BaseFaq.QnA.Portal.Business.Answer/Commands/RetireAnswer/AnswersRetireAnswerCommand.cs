using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.RetireAnswer;

public sealed class AnswersRetireAnswerCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}