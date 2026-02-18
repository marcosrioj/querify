using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItem;

public sealed class FaqItemsGetFaqItemQuery : IRequest<FaqItemDto?>
{
    public required Guid Id { get; set; }
}