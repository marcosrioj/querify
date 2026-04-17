using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.ValidateAnswer;

public sealed class AnswersValidateAnswerCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}