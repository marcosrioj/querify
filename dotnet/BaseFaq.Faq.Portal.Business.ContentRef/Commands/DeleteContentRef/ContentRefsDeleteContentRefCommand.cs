using MediatR;

namespace BaseFaq.Faq.Portal.Business.ContentRef.Commands.DeleteContentRef;

public sealed class ContentRefsDeleteContentRefCommand : IRequest
{
    public required Guid Id { get; set; }
}