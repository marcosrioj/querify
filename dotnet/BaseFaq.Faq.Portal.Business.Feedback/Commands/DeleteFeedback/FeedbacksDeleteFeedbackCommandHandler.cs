using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Faq.Portal.Business.Feedback.Commands.DeleteFeedback;

public class FeedbacksDeleteFeedbackCommandHandler(FaqDbContext dbContext) : IRequestHandler<FeedbacksDeleteFeedbackCommand>
{
    public async Task Handle(FeedbacksDeleteFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = await dbContext.Feedbacks
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (feedback is null)
        {
            throw new ApiErrorException(
                $"Feedback '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.Feedbacks.Remove(feedback);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}