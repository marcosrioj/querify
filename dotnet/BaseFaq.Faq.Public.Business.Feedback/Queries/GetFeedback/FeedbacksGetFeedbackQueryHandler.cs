using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Faq.Dtos.Feedback;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Public.Business.Feedback.Queries.GetFeedback;

public class FeedbacksGetFeedbackQueryHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FeedbacksGetFeedbackQuery, FeedbackDto?>
{
    public async Task<FeedbackDto?> Handle(FeedbacksGetFeedbackQuery request, CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;

        return await dbContext.Feedbacks
            .AsNoTracking()
            .Where(feedback => feedback.TenantId == tenantId && feedback.Id == request.Id)
            .Select(feedback => new FeedbackDto
            {
                Id = feedback.Id,
                Like = feedback.Like,
                UserPrint = feedback.UserPrint,
                Ip = feedback.Ip,
                UserAgent = feedback.UserAgent,
                UnLikeReason = feedback.UnLikeReason,
                FaqItemId = feedback.FaqItemId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}