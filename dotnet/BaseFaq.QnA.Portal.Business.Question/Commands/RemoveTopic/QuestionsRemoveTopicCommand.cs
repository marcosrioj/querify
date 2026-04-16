using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RemoveTopic;

public sealed class QuestionsRemoveTopicCommand : IRequest
{
    public Guid QuestionId { get; set; }
    public Guid TopicId { get; set; }
}
