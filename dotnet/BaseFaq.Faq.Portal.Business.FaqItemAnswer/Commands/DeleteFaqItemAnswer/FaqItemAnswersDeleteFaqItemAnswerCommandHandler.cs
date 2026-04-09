using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.DeleteFaqItemAnswer;

public class FaqItemAnswersDeleteFaqItemAnswerCommandHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqItemAnswersDeleteFaqItemAnswerCommand>
{
    public async Task Handle(FaqItemAnswersDeleteFaqItemAnswerCommand request, CancellationToken cancellationToken)
    {
        var faqItemAnswer = await dbContext.FaqItemAnswers.FirstOrDefaultAsync(
            entity => entity.Id == request.Id,
            cancellationToken);
        if (faqItemAnswer is null)
        {
            return;
        }

        dbContext.FaqItemAnswers.Remove(faqItemAnswer);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
