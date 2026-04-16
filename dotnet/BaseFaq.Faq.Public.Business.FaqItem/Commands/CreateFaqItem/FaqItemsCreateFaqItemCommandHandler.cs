using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Public.Business.FaqItem.Commands.CreateFaqItem;

public class FaqItemsCreateFaqItemCommandHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FaqItemsCreateFaqItemCommand, Guid>
{
    public async Task<Guid> Handle(FaqItemsCreateFaqItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        await GetFaqOrThrowAsync(request.FaqId, tenantId, cancellationToken);
        var faqItem = await CreateFaqItemAsync(request, tenantId, cancellationToken);
        return faqItem.Id;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private async Task<Common.Persistence.FaqDb.Entities.Faq> GetFaqOrThrowAsync(
        Guid faqId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var faq = await dbContext.Faqs
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == faqId, cancellationToken);

        if (faq is null)
        {
            throw new ApiErrorException(
                $"FAQ '{faqId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return faq;
    }

    private async Task<Common.Persistence.FaqDb.Entities.FaqItem> CreateFaqItemAsync(
        FaqItemsCreateFaqItemCommand request,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var faqItem = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = request.Question,
            AdditionalInfo = request.AdditionalInfo,
            CtaTitle = request.CtaTitle,
            CtaUrl = request.CtaUrl,
            Sort = request.Sort,
            IsActive = request.IsActive,
            FaqId = request.FaqId,
            ContentRefId = request.ContentRefId,
            TenantId = tenantId
        };

        await dbContext.FaqItems.AddAsync(faqItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return faqItem;
    }
}
