using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqContentRef;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqContentRefList;

public sealed class FaqContentRefsGetFaqContentRefListQuery : IRequest<PagedResultDto<FaqContentRefDto>>
{
    public required FaqContentRefGetAllRequestDto Request { get; set; }
}