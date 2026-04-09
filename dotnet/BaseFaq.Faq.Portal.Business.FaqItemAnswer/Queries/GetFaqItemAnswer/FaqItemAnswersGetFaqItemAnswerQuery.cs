using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswer;

public sealed class FaqItemAnswersGetFaqItemAnswerQuery : IRequest<FaqItemAnswerDto?>
{
    public required Guid Id { get; set; }
}
