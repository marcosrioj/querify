using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqTag;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqTagList;

public sealed class FaqTagsGetFaqTagListQuery : IRequest<PagedResultDto<FaqTagDto>>
{
    public required FaqTagGetAllRequestDto Request { get; set; }
}