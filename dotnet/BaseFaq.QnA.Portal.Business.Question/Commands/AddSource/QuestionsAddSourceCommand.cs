using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands;

public sealed class QuestionsAddSourceCommand : IRequest<Guid>
{
    public required QuestionSourceLinkCreateRequestDto Request { get; set; }
}
