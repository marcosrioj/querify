using BaseFaq.Models.QnA.Dtos.Topic;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands.CreateTopic;

public sealed class TopicsCreateTopicCommand : IRequest<Guid>
{
    public required TopicCreateRequestDto Request { get; set; }
}