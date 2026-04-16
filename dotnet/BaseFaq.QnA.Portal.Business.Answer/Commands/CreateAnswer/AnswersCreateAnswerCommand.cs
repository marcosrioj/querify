using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersCreateAnswerCommand : IRequest<Guid>
{
    public required AnswerCreateRequestDto Request { get; set; }
}
