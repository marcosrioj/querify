using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Tag;

namespace Querify.QnA.Portal.Business.Tag.Abstractions;

public interface ITagService
{
    Task<Guid> Create(TagCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<TagDto>> GetAll(TagGetAllRequestDto requestDto, CancellationToken token);
    Task<TagDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, TagUpdateRequestDto dto, CancellationToken token);
}