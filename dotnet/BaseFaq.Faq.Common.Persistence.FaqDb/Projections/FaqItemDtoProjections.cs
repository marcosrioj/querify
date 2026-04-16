using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Projections;

public static class FaqItemDtoProjections
{
    public static IQueryable<FaqItemDto> SelectPortalFaqItemDtos(this IQueryable<FaqItem> query)
    {
        return query.Select(item => new FaqItemDto
        {
            Id = item.Id,
            Question = item.Question,
            ShortAnswer = item.Answers
                .OrderBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.VoteScore)
                .ThenBy(answer => answer.Id)
                .Select(answer => answer.ShortAnswer)
                .FirstOrDefault() ?? string.Empty,
            Answer = item.Answers
                .OrderBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.VoteScore)
                .ThenBy(answer => answer.Id)
                .Select(answer => answer.Answer)
                .FirstOrDefault(),
            Answers = item.Answers
                .OrderBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.VoteScore)
                .ThenBy(answer => answer.Id)
                .Select(answer => new FaqItemAnswerDto
                {
                    Id = answer.Id,
                    ShortAnswer = answer.ShortAnswer,
                    Answer = answer.Answer,
                    Sort = answer.Sort,
                    VoteScore = answer.VoteScore,
                    IsActive = answer.IsActive,
                    FaqItemId = answer.FaqItemId
                })
                .ToList(),
            AdditionalInfo = item.AdditionalInfo,
            CtaTitle = item.CtaTitle,
            CtaUrl = item.CtaUrl,
            Sort = item.Sort,
            FeedbackScore = item.FeedbackScore,
            ConfidenceScore = item.ConfidenceScore,
            IsActive = item.IsActive,
            FaqId = item.FaqId,
            ContentRefId = item.ContentRefId
        });
    }

    public static IQueryable<FaqItemDto> SelectPublicFaqItemDtos(this IQueryable<FaqItem> query)
    {
        return query.Select(item => new FaqItemDto
        {
            Id = item.Id,
            Question = item.Question,
            ShortAnswer = item.Answers
                .Where(answer => answer.IsActive)
                .OrderBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.VoteScore)
                .ThenBy(answer => answer.Id)
                .Select(answer => answer.ShortAnswer)
                .FirstOrDefault() ?? string.Empty,
            Answer = item.Answers
                .Where(answer => answer.IsActive)
                .OrderBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.VoteScore)
                .ThenBy(answer => answer.Id)
                .Select(answer => answer.Answer)
                .FirstOrDefault(),
            Answers = item.Answers
                .Where(answer => answer.IsActive)
                .OrderBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.VoteScore)
                .ThenBy(answer => answer.Id)
                .Select(answer => new FaqItemAnswerDto
                {
                    Id = answer.Id,
                    ShortAnswer = answer.ShortAnswer,
                    Answer = answer.Answer,
                    Sort = answer.Sort,
                    VoteScore = answer.VoteScore,
                    IsActive = answer.IsActive,
                    FaqItemId = answer.FaqItemId
                })
                .ToList(),
            AdditionalInfo = item.AdditionalInfo,
            CtaTitle = item.CtaTitle,
            CtaUrl = item.CtaUrl,
            Sort = item.Sort,
            FeedbackScore = item.FeedbackScore,
            ConfidenceScore = item.ConfidenceScore,
            IsActive = item.IsActive,
            FaqId = item.FaqId,
            ContentRefId = item.ContentRefId
        });
    }
}
