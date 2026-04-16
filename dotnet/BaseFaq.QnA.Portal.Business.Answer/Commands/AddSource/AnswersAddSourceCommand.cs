using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersAddSourceCommand : IRequest<Guid>
{
    public required AnswerSourceLinkCreateRequestDto Request { get; set; }
}
