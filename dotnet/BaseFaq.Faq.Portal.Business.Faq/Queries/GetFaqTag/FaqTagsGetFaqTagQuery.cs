using BaseFaq.Models.Faq.Dtos.FaqTag;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqTag;

public sealed class FaqTagsGetFaqTagQuery : IRequest<FaqTagDto?>
{
    public required Guid Id { get; set; }
}