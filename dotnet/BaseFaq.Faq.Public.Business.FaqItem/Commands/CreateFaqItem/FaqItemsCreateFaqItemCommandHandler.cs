using System.Net;
using BaseFaq.AI.Common.Contracts.Matching;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.Faq.Public.Business.FaqItem.Commands.CreateFaqItem;

public class FaqItemsCreateFaqItemCommandHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<FaqItemsCreateFaqItemCommand, Guid>
{
    private const int MaxQueryLength = 2000;
    private const int MaxLanguageLength = 16;

    public async Task<Guid> Handle(FaqItemsCreateFaqItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var faq = await GetFaqOrThrowAsync(request.FaqId, tenantId, cancellationToken);
        var faqItem = await CreateFaqItemAsync(request, tenantId, cancellationToken);
        ValidateMatchingInputs(request.Question, faq.Language);
        await PublishMatchingRequestedAsync(request, faqItem, faq.Language, tenantId, cancellationToken);
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
            ShortAnswer = request.ShortAnswer,
            Answer = request.Answer,
            AdditionalInfo = request.AdditionalInfo,
            CtaTitle = request.CtaTitle,
            CtaUrl = request.CtaUrl,
            Sort = request.Sort,
            VoteScore = request.VoteScore,
            AiConfidenceScore = request.AiConfidenceScore,
            IsActive = request.IsActive,
            FaqId = request.FaqId,
            ContentRefId = request.ContentRefId,
            TenantId = tenantId
        };

        await dbContext.FaqItems.AddAsync(faqItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return faqItem;
    }

    private async Task PublishMatchingRequestedAsync(
        FaqItemsCreateFaqItemCommand request,
        Common.Persistence.FaqDb.Entities.FaqItem faqItem,
        string language,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new FaqMatchingRequestedV1
        {
            CorrelationId = Guid.NewGuid(),
            TenantId = tenantId,
            FaqItemId = faqItem.Id,
            RequestedByUserId = Guid.Empty,
            Query = request.Question,
            Language = language,
            IdempotencyKey = $"faqitem-create-{faqItem.Id:N}",
            RequestedUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    private static void ValidateMatchingInputs(string query, string language)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length > MaxQueryLength)
        {
            throw new ApiErrorException(
                $"Question is required and must have at most {MaxQueryLength} characters to request matching.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(language) || language.Length > MaxLanguageLength)
        {
            throw new ApiErrorException(
                $"FAQ language is required and must have at most {MaxLanguageLength} characters to request matching.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }
    }
}