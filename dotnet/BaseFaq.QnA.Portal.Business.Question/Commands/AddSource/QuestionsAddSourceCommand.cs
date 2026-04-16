using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.AddSource;

public sealed class QuestionsAddSourceCommand : IRequest<Guid>
{
    public required QuestionSourceLinkCreateRequestDto Request { get; set; }
}