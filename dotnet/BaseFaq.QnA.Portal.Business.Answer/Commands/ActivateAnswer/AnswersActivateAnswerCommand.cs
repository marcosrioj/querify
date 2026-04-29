using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.ActivateAnswer;

public sealed class AnswersActivateAnswerCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}
