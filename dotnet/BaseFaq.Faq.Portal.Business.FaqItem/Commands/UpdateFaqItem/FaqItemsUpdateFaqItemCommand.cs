using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Commands.UpdateFaqItem;

public sealed class FaqItemsUpdateFaqItemCommand : IRequest
{
    public required Guid Id { get; set; }
    public required string Question { get; set; }
    public required string ShortAnswer { get; set; }
    public string? Answer { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? CtaTitle { get; set; }
    public string? CtaUrl { get; set; }
    public required int Sort { get; set; }
    public required bool IsActive { get; set; }
    public required Guid FaqId { get; set; }
    public Guid? ContentRefId { get; set; }
}