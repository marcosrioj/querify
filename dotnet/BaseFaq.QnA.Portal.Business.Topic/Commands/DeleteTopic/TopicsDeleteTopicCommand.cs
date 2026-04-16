using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands.DeleteTopic;

public sealed class TopicsDeleteTopicCommand : IRequest
{
    public Guid Id { get; set; }
}