using Querify.Models.QnA.Dtos.Answer;
using MediatR;

namespace Querify.QnA.Portal.Business.Answer.Commands.AddSource;

public sealed class AnswersAddSourceCommand : IRequest<Guid>
{
    public required AnswerSourceLinkCreateRequestDto Request { get; set; }
}