using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using BaseFaq.Faq.Portal.Business.Feedback.Helpers;

namespace BaseFaq.Faq.Portal.Business.Feedback.Commands.UpdateFeedback;

public class FeedbacksUpdateFeedbackCommandHandler(
    FaqDbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FeedbacksUpdateFeedbackCommand>
{
    public async Task Handle(FeedbacksUpdateFeedbackCommand request, CancellationToken cancellationToken)
    {
        if (!request.Like && request.UnLikeReason is null)
        {
            throw new ApiErrorException(
                "UnLikeReason is required when Like is false.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var feedback = await dbContext.Feedbacks.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (feedback is null)
        {
            throw new ApiErrorException(
                $"Feedback '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var identity = FeedbackRequestContext.GetIdentity(sessionService, httpContextAccessor);

        feedback.Like = request.Like;
        feedback.UserPrint = identity.UserPrint;
        feedback.Ip = identity.Ip;
        feedback.UserAgent = identity.UserAgent;
        feedback.UnLikeReason = request.UnLikeReason;
        feedback.FaqItemId = request.FaqItemId;

        dbContext.Feedbacks.Update(feedback);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}