using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;

namespace BaseFaq.QnA.Portal.Business.ThreadActivity.Abstractions;

public interface IThreadActivityService
{
    Task<PagedResultDto<ThreadActivityDto>> GetAll(ThreadActivityGetAllRequestDto requestDto, CancellationToken token);
    Task<ThreadActivityDto> GetById(Guid id, CancellationToken token);
}
