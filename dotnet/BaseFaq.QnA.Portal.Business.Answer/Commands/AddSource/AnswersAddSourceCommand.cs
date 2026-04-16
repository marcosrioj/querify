using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.AddSource;

public sealed class AnswersAddSourceCommand : IRequest<Guid>
{
    public required AnswerSourceLinkCreateRequestDto Request { get; set; }
}
