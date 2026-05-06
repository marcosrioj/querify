using Querify.Models.QnA.Dtos.Answer;
using MediatR;

namespace Querify.QnA.Portal.Business.Answer.Commands.CreateAnswer;

public sealed class AnswersCreateAnswerCommand : IRequest<Guid>
{
    public required AnswerCreateRequestDto Request { get; set; }
}