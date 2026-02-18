using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaqTag;

public sealed class FaqTagsCreateFaqTagCommand : IRequest<Guid>
{
    public required Guid FaqId { get; set; }
    public required Guid TagId { get; set; }
}