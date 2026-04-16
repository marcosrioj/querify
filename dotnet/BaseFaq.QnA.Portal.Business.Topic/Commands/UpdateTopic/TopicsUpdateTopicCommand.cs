using BaseFaq.Models.QnA.Dtos.Topic;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands.UpdateTopic;

public sealed class TopicsUpdateTopicCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required TopicUpdateRequestDto Request { get; set; }
}