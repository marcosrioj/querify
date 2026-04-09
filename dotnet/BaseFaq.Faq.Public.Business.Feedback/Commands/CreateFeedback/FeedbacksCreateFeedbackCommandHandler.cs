using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Public.Business.Feedback.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Public.Business.Feedback.Commands.CreateFeedback;

public class FeedbacksCreateFeedbackCommandHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FeedbacksCreateFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(FeedbacksCreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        ValidateUnlikeReason(request);
        var identity = ResolveFeedbackIdentity();
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);

        var existing = await dbContext.Feedbacks
            .FirstOrDefaultAsync(
                feedback => feedback.TenantId == tenantId &&
                        feedback.FaqItemId == request.FaqItemId &&
                        feedback.UserPrint == identity.UserPrint,
                cancellationToken);

        if (existing is not null)
        {
            if (existing.Like == request.Like)
            {
                return existing.Id;
            }

            return await UpdateExistingFeedbackAsync(request, tenantId, identity, existing, cancellationToken);
        }

        return await CreateFeedbackAsync(request, tenantId, identity, cancellationToken);
    }

    private static void ValidateUnlikeReason(FeedbacksCreateFeedbackCommand request)
    {
        if (!request.Like && request.UnLikeReason is null)
        {
            throw new ApiErrorException(
                "UnLikeReason is required when Like is false.",
                errorCode: (int)HttpStatusCode.UnprocessableEntity);
        }
    }

    private FeedbackRequestIdentity ResolveFeedbackIdentity()
    {
        return FeedbackRequestContext.GetIdentity(httpContextAccessor);
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private async Task<Guid> UpdateExistingFeedbackAsync(
        FeedbacksCreateFeedbackCommand request,
        Guid tenantId,
        FeedbackRequestIdentity identity,
        Common.Persistence.FaqDb.Entities.Feedback existing,
        CancellationToken cancellationToken)
    {
        var faqItem = await GetFaqItemOrThrowAsync(tenantId, request.FaqItemId, cancellationToken);

        existing.Like = request.Like;
        existing.Ip = identity.Ip;
        existing.UserAgent = identity.UserAgent;
        existing.UnLikeReason = request.UnLikeReason;
        faqItem.FeedbackScore += request.Like ? 2 : -2;
        await dbContext.SaveChangesAsync(cancellationToken);

        return existing.Id;
    }

    private async Task<Guid> CreateFeedbackAsync(
        FeedbacksCreateFeedbackCommand request,
        Guid tenantId,
        FeedbackRequestIdentity identity,
        CancellationToken cancellationToken)
    {
        var faqItemForCreate = await GetFaqItemOrThrowAsync(tenantId, request.FaqItemId, cancellationToken);
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

        faqItemForCreate.FeedbackScore = await dbContext.Feedbacks
            .Where(v => v.TenantId == tenantId && v.FaqItemId == request.FaqItemId)
            .SumAsync(v => v.Like ? 1 : -1, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }

    private async Task<Common.Persistence.FaqDb.Entities.FaqItem> GetFaqItemOrThrowAsync(
        Guid tenantId,
        Guid faqItemId,
        CancellationToken cancellationToken)
    {
        var faqItem = await dbContext.FaqItems
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == faqItemId, cancellationToken);

        if (faqItem is null)
        {
            throw new ApiErrorException(
                $"FAQ item '{faqItemId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return faqItem;
    }
}