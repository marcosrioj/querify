using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Topic;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Queries.GetTopicList;

public sealed class TopicsGetTopicListQuery : IRequest<PagedResultDto<TopicDto>>
{
    public required TopicGetAllRequestDto Request { get; set; }
}