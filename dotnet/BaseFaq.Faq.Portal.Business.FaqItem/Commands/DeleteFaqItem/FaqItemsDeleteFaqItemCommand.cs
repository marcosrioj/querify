using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Commands.DeleteFaqItem;

public sealed class FaqItemsDeleteFaqItemCommand : IRequest
{
    public required Guid Id { get; set; }
}