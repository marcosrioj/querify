using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Topic;

namespace BaseFaq.QnA.Portal.Business.Topic.Abstractions;

public interface ITopicService
{
    Task<Guid> Create(TopicCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<TopicDto>> GetAll(TopicGetAllRequestDto requestDto, CancellationToken token);
    Task<TopicDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, TopicUpdateRequestDto dto, CancellationToken token);
}
