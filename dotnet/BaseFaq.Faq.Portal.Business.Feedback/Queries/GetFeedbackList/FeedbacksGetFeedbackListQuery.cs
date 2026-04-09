using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Feedback;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedbackList;

public sealed class FeedbacksGetFeedbackListQuery : IRequest<PagedResultDto<FeedbackDto>>
{
    public required FeedbackGetAllRequestDto Request { get; set; }
}