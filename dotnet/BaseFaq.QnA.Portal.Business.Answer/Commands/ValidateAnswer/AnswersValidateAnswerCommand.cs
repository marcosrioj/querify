using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersValidateAnswerCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
