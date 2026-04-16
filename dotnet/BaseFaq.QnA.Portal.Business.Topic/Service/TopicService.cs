using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Portal.Business.Topic.Abstractions;
using BaseFaq.QnA.Portal.Business.Topic.Commands;
using BaseFaq.QnA.Portal.Business.Topic.Queries;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Service;

public sealed class TopicService(IMediator mediator) : ITopicService
{
    public Task<Guid> Create(TopicCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new TopicsCreateTopicCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<TopicDto>> GetAll(TopicGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new TopicsGetTopicListQuery { Request = requestDto }, token);
    }

    public Task<TopicDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new TopicsGetTopicQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, TopicUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new TopicsUpdateTopicCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new TopicsDeleteTopicCommand { Id = id }, token);
    }
}
