using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.DeleteFaqTag;

public sealed class FaqTagsDeleteFaqTagCommand : IRequest
{
    public required Guid Id { get; set; }
}