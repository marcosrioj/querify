using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.UpdateAnswer;

public sealed class AnswersUpdateAnswerCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
    public required AnswerUpdateRequestDto Request { get; set; }
}