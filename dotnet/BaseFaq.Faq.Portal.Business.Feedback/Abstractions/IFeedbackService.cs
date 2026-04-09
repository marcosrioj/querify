using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Feedback;

namespace BaseFaq.Faq.Portal.Business.Feedback.Abstractions;

public interface IFeedbackService
{
    Task<Guid> Create(FeedbackCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<FeedbackDto>> GetAll(FeedbackGetAllRequestDto requestDto, CancellationToken token);
    Task<FeedbackDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, FeedbackUpdateRequestDto dto, CancellationToken token);
}