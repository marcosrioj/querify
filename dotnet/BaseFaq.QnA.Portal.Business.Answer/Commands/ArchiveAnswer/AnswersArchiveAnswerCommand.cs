using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.ArchiveAnswer;

public sealed class AnswersArchiveAnswerCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}
