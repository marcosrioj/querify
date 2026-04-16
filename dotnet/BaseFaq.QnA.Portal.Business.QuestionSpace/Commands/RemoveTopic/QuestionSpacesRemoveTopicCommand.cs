using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveTopic;

public sealed class QuestionSpacesRemoveTopicCommand : IRequest
{
    public Guid QuestionSpaceId { get; set; }
    public Guid TopicId { get; set; }
}
