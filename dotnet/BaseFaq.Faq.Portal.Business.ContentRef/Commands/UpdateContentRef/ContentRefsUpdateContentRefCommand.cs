using BaseFaq.Models.Faq.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.ContentRef.Commands.UpdateContentRef;

public sealed class ContentRefsUpdateContentRefCommand : IRequest
{
    public required Guid Id { get; set; }
    public required ContentRefKind Kind { get; set; }
    public required string Locator { get; set; }
    public string? Label { get; set; }
    public string? Scope { get; set; }
}