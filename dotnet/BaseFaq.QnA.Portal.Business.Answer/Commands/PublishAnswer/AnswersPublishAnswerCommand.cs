using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.PublishAnswer;

public sealed class AnswersPublishAnswerCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}