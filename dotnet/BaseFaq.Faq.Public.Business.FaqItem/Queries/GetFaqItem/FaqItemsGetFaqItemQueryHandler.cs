using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Public.Business.FaqItem.Queries.GetFaqItem;

public class FaqItemsGetFaqItemQueryHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FaqItemsGetFaqItemQuery, FaqItemDto?>
{
    public async Task<FaqItemDto?> Handle(FaqItemsGetFaqItemQuery request, CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;

        return await dbContext.FaqItems
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.Id == request.Id)
            .Select(item => new FaqItemDto
            {
                Id = item.Id,
                Question = item.Question,
                ShortAnswer = item.ShortAnswer,
                Answer = item.Answer,
                AdditionalInfo = item.AdditionalInfo,
                CtaTitle = item.CtaTitle,
                CtaUrl = item.CtaUrl,
                Sort = item.Sort,
                FeedbackScore = item.FeedbackScore,
                AiConfidenceScore = item.AiConfidenceScore,
                IsActive = item.IsActive,
                FaqId = item.FaqId,
                ContentRefId = item.ContentRefId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}