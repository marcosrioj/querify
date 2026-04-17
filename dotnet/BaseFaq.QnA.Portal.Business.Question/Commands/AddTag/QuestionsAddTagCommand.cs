using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.AddTag;

public sealed class QuestionsAddTagCommand : IRequest<Guid>
{
    public required QuestionTagCreateRequestDto Request { get; set; }
}