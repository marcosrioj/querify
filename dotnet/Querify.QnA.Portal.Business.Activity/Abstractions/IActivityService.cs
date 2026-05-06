using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Activity;

namespace Querify.QnA.Portal.Business.Activity.Abstractions;

public interface IActivityService
{
    Task<PagedResultDto<ActivityDto>> GetAll(ActivityGetAllRequestDto requestDto, CancellationToken token);
    Task<ActivityDto> GetById(Guid id, CancellationToken token);
}