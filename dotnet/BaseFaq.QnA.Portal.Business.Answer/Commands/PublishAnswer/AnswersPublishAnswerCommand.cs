using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.PublishAnswer;

public sealed class AnswersPublishAnswerCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
