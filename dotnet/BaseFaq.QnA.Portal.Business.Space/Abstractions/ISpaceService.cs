using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Space;

namespace BaseFaq.QnA.Portal.Business.Space.Abstractions;

public interface ISpaceService
{
    Task<Guid> Create(SpaceCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<SpaceDto>> GetAll(SpaceGetAllRequestDto requestDto, CancellationToken token);
    Task<SpaceDetailDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, SpaceUpdateRequestDto dto, CancellationToken token);
    Task<Guid> AddTag(SpaceTagCreateRequestDto dto, CancellationToken token);
    Task RemoveTag(Guid spaceId, Guid tagId, CancellationToken token);
    Task<Guid> AddCuratedSource(SpaceSourceCreateRequestDto dto, CancellationToken token);
    Task RemoveCuratedSource(Guid spaceId, Guid sourceId, CancellationToken token);
}