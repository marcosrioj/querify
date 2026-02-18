using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaqContentRef;

public sealed class FaqContentRefsCreateFaqContentRefCommand : IRequest<Guid>
{
    public required Guid FaqId { get; set; }
    public required Guid ContentRefId { get; set; }
}