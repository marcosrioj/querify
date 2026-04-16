using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Activity;

namespace BaseFaq.QnA.Portal.Business.Activity.Abstractions;

public interface IActivityService
{
    Task<PagedResultDto<ActivityDto>> GetAll(ActivityGetAllRequestDto requestDto, CancellationToken token);
    Task<ActivityDto> GetById(Guid id, CancellationToken token);
}