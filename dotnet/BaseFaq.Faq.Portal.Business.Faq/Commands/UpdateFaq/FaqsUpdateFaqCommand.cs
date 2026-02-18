using BaseFaq.Models.Faq.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaq;

public sealed class FaqsUpdateFaqCommand : IRequest
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Language { get; set; }
    public required FaqStatus Status { get; set; }
    public required FaqSortStrategy SortStrategy { get; set; }
    public bool CtaEnabled { get; set; }
    public CtaTarget CtaTarget { get; set; } = CtaTarget.Self;
}