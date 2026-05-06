using Querify.Models.QnA.Dtos.Question;
using MediatR;

namespace Querify.QnA.Portal.Business.Question.Commands.AddSource;

public sealed class QuestionsAddSourceCommand : IRequest<Guid>
{
    public required QuestionSourceLinkCreateRequestDto Request { get; set; }
}