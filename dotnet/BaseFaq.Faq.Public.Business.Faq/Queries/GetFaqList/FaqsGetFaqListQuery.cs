using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Faq;
using MediatR;

namespace BaseFaq.Faq.Public.Business.Faq.Queries.GetFaqList;

public sealed class FaqsGetFaqListQuery : IRequest<PagedResultDto<FaqDetailDto>>
{
    public required FaqGetAllRequestDto Request { get; set; }
}