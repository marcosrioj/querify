using BaseFaq.Models.Faq.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaq;

public sealed class FaqsCreateFaqCommand : IRequest<Guid>
{
    public required string Name { get; set; }
    public required string Language { get; set; }
    public required FaqStatus Status { get; set; }
}
