using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItemList;

public sealed class FaqItemsGetFaqItemListQuery : IRequest<PagedResultDto<FaqItemDto>>
{
    public required FaqItemGetAllRequestDto Request { get; set; }
}