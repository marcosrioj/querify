using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswerList;

public sealed class FaqItemAnswersGetFaqItemAnswerListQuery : IRequest<PagedResultDto<FaqItemAnswerDto>>
{
    public required FaqItemAnswerGetAllRequestDto Request { get; set; }
}
