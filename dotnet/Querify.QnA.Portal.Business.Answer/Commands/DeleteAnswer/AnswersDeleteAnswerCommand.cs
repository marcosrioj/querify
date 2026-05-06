using MediatR;

namespace Querify.QnA.Portal.Business.Answer.Commands.DeleteAnswer;

public sealed class AnswersDeleteAnswerCommand : IRequest
{
    public required Guid Id { get; set; }
}