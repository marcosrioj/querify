using BaseFaq.Models.QnA.Dtos.Topic;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Queries.GetTopic;

public sealed class TopicsGetTopicQuery : IRequest<TopicDto>
{
    public Guid Id { get; set; }
}