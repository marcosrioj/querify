using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;
using BaseFaq.Faq.Portal.Business.Feedback.Helpers;

namespace BaseFaq.Faq.Portal.Business.Feedback.Commands.CreateFeedback;

public class FeedbacksCreateFeedbackCommandHandler(
    FaqDbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FeedbacksCreateFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(FeedbacksCreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        if (!request.Like && request.UnLikeReason is null)
        {
            throw new ApiErrorException(
                "UnLikeReason is required when Like is false.",
                errorCode: (int)HttpStatusCode.UnprocessableEntity);
        }

        var identity = FeedbackRequestContext.GetIdentity(sessionService, httpContextAccessor);
        var tenantId = sessionService.GetTenantId(AppEnum.Faq);

        var feedback = new Common.Persistence.FaqDb.Entities.Feedback
        {
            Like = request.Like,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            UnLikeReason = request.UnLikeReason,
            TenantId = tenantId,
            FaqItemId = request.FaqItemId
        };

        await dbContext.Feedbacks.AddAsync(feedback, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}