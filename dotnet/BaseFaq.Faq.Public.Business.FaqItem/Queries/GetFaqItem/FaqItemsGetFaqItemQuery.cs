using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;

namespace BaseFaq.Faq.Public.Business.FaqItem.Queries.GetFaqItem;

public sealed class FaqItemsGetFaqItemQuery : IRequest<FaqItemDto?>
{
    public required Guid Id { get; set; }
}