using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersRetireAnswerCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
