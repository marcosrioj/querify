using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Space;

namespace Querify.QnA.Public.Business.Space.Abstractions;

public interface ISpaceService
{
    Task<PagedResultDto<SpaceDto>> GetAll(SpaceGetAllRequestDto requestDto, CancellationToken token);
    Task<SpaceDto> GetById(Guid id, CancellationToken token);
    Task<SpaceDto> GetBySlug(string slug, CancellationToken token);
}
