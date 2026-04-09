using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.DeleteFaqItemAnswer;

public sealed class FaqItemAnswersDeleteFaqItemAnswerCommand : IRequest
{
    public required Guid Id { get; set; }
}
