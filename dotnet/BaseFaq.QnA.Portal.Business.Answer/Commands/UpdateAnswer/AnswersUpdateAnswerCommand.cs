using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.UpdateAnswer;

public sealed class AnswersUpdateAnswerCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required AnswerUpdateRequestDto Request { get; set; }
}