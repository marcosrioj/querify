using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.CreateFaqItemAnswer;

public sealed class FaqItemAnswersCreateFaqItemAnswerCommand : IRequest<Guid>
{
    public required string ShortAnswer { get; set; }
    public string? Answer { get; set; }
    public required int Sort { get; set; }
    public required bool IsActive { get; set; }
    public required Guid FaqItemId { get; set; }
}
