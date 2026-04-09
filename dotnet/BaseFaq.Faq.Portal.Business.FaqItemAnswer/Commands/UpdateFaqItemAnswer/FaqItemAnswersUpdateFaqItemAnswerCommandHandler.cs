using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.UpdateFaqItemAnswer;

public class FaqItemAnswersUpdateFaqItemAnswerCommandHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqItemAnswersUpdateFaqItemAnswerCommand>
{
    public async Task Handle(FaqItemAnswersUpdateFaqItemAnswerCommand request, CancellationToken cancellationToken)
    {
        var faqItemAnswer = await dbContext.FaqItemAnswers.FirstOrDefaultAsync(
            entity => entity.Id == request.Id,
            cancellationToken);
        if (faqItemAnswer is null)
        {
            throw new ApiErrorException(
                $"FAQ item answer '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        faqItemAnswer.ShortAnswer = request.ShortAnswer;
        faqItemAnswer.Answer = request.Answer;
        faqItemAnswer.Sort = request.Sort;
        faqItemAnswer.IsActive = request.IsActive;
        faqItemAnswer.FaqItemId = request.FaqItemId;

        dbContext.FaqItemAnswers.Update(faqItemAnswer);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
