using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Space;

namespace BaseFaq.QnA.Public.Business.Space.Abstractions;

public interface ISpaceService
{
    Task<PagedResultDto<SpaceDto>> GetAll(SpaceGetAllRequestDto requestDto, CancellationToken token);
    Task<SpaceDto> GetById(Guid id, CancellationToken token);
    Task<SpaceDto> GetByKey(string key, CancellationToken token);
}