using BaseFaq.Models.Faq.Dtos.ContentRef;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.ContentRef.Queries.GetContentRef;

public sealed class ContentRefsGetContentRefQuery : IRequest<ContentRefDto?>
{
    public required Guid Id { get; set; }
}