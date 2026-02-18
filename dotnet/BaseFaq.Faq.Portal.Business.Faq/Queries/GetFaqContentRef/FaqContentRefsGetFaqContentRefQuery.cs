using BaseFaq.Models.Faq.Dtos.FaqContentRef;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqContentRef;

public sealed class FaqContentRefsGetFaqContentRefQuery : IRequest<FaqContentRefDto?>
{
    public required Guid Id { get; set; }
}