using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItem;

public class FaqItemsGetFaqItemQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqItemsGetFaqItemQuery, FaqItemDto?>
{
    public async Task<FaqItemDto?> Handle(FaqItemsGetFaqItemQuery request, CancellationToken cancellationToken)
    {
        var faqItem = await dbContext.FaqItems
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (faqItem is null)
        {
            return null;
        }

        return new FaqItemDto
        {
            Id = faqItem.Id,
            Question = faqItem.Question,
            ShortAnswer = faqItem.ShortAnswer,
            Answer = faqItem.Answer,
            AdditionalInfo = faqItem.AdditionalInfo,
            CtaTitle = faqItem.CtaTitle,
            CtaUrl = faqItem.CtaUrl,
            Sort = faqItem.Sort,
            FeedbackScore = faqItem.FeedbackScore,
            AiConfidenceScore = faqItem.AiConfidenceScore,
            IsActive = faqItem.IsActive,
            FaqId = faqItem.FaqId,
            ContentRefId = faqItem.ContentRefId
        };
    }
}