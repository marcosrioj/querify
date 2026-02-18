using MediatR;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.DeleteFaq;

public sealed class FaqsDeleteFaqCommand : IRequest
{
    public required Guid Id { get; set; }
}