using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands;

public sealed class QuestionsAddTopicCommand : IRequest<Guid>
{
    public required QuestionTopicCreateRequestDto Request { get; set; }
}
