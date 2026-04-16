using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersDeleteAnswerCommand : IRequest
{
    public Guid Id { get; set; }
}
