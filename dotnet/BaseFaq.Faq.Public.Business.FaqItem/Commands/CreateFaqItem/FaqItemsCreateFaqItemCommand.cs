using MediatR;

namespace BaseFaq.Faq.Public.Business.FaqItem.Commands.CreateFaqItem;

public sealed class FaqItemsCreateFaqItemCommand : IRequest<Guid>
{
    public required string Question { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? CtaTitle { get; set; }
    public string? CtaUrl { get; set; }
    public required int Sort { get; set; }
    public required bool IsActive { get; set; }
    public required Guid FaqId { get; set; }
    public Guid? ContentRefId { get; set; }
}
