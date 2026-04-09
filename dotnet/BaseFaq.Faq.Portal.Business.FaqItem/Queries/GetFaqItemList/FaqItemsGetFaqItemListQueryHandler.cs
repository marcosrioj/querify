using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItemList;

public class FaqItemsGetFaqItemListQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqItemsGetFaqItemListQuery, PagedResultDto<FaqItemDto>>
{
    public async Task<PagedResultDto<FaqItemDto>> Handle(
        FaqItemsGetFaqItemListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = BuildSortedQuery(request);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await LoadItemsAsync(query, request, cancellationToken);
        return new PagedResultDto<FaqItemDto>(totalCount, items);
    }

    private IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> BuildSortedQuery(FaqItemsGetFaqItemListQuery request)
    {
        var query = dbContext.FaqItems.AsNoTracking();
        query = ApplyFilters(query, request.Request);
        return ApplySorting(query, request.Request.Sorting);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> ApplyFilters(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> query,
        FaqItemGetAllRequestDto request)
    {
        if (request.FaqId.HasValue)
        {
            query = query.Where(item => item.FaqId == request.FaqId.Value);
        }

        if (request.ContentRefId.HasValue)
        {
            query = query.Where(item => item.ContentRefId == request.ContentRefId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(item => item.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var pattern = $"%{request.SearchText.Trim()}%";
            query = query.Where(item =>
                EF.Functions.ILike(item.Question, pattern) ||
                EF.Functions.ILike(item.ShortAnswer, pattern) ||
                (item.Answer != null && EF.Functions.ILike(item.Answer, pattern)) ||
                (item.AdditionalInfo != null && EF.Functions.ILike(item.AdditionalInfo, pattern)));
        }

        return query;
    }

    private static async Task<List<FaqItemDto>> LoadItemsAsync(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> query,
        FaqItemsGetFaqItemListQuery request,
        CancellationToken cancellationToken)
    {
        return await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
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
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> ApplySorting(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(item => item.UpdatedDate);
        }

        IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>? orderedQuery = null;
        var fields = sorting.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            var parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            var fieldName = parts[0];
            var desc = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

            orderedQuery = ApplyOrder(orderedQuery ?? query, fieldName, desc, orderedQuery is null);
        }

        return orderedQuery ?? query.OrderByDescending(item => item.UpdatedDate);
    }

    private static IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem> ApplyOrder(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItem> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "question" => isFirst
                ? (desc ? query.OrderByDescending(item => item.Question) : query.OrderBy(item => item.Question))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.Question)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.Question)),
            "answer" => isFirst
                ? (desc ? query.OrderByDescending(item => item.Answer) : query.OrderBy(item => item.Answer))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.Answer)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.Answer)),
            "shortanswer" => isFirst
                ? (desc
                    ? query.OrderByDescending(item => item.ShortAnswer)
                    : query.OrderBy(item => item.ShortAnswer))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.ShortAnswer)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenBy(item =>
                        item.ShortAnswer)),
            "additionalinfo" => isFirst
                ? (desc
                    ? query.OrderByDescending(item => item.AdditionalInfo)
                    : query.OrderBy(item => item.AdditionalInfo))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.AdditionalInfo)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenBy(item =>
                        item.AdditionalInfo)),
            "ctatitle" => isFirst
                ? (desc ? query.OrderByDescending(item => item.CtaTitle) : query.OrderBy(item => item.CtaTitle))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.CtaTitle)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.CtaTitle)),
            "ctaurl" => isFirst
                ? (desc ? query.OrderByDescending(item => item.CtaUrl) : query.OrderBy(item => item.CtaUrl))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.CtaUrl)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.CtaUrl)),
            "sort" => isFirst
                ? (desc ? query.OrderByDescending(item => item.Sort) : query.OrderBy(item => item.Sort))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.Sort)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenBy(item => item.Sort)),
            "feedbackscore" => isFirst
                ? (desc ? query.OrderByDescending(item => item.FeedbackScore) : query.OrderBy(item => item.FeedbackScore))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.FeedbackScore)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenBy(item =>
                        item.FeedbackScore)),
            "aiconfidencescore" => isFirst
                ? (desc
                    ? query.OrderByDescending(item => item.AiConfidenceScore)
                    : query.OrderBy(item => item.AiConfidenceScore))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.AiConfidenceScore)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.AiConfidenceScore)),
            "isactive" => isFirst
                ? (desc ? query.OrderByDescending(item => item.IsActive) : query.OrderBy(item => item.IsActive))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.IsActive)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.IsActive)),
            "faqid" => isFirst
                ? (desc ? query.OrderByDescending(item => item.FaqId) : query.OrderBy(item => item.FaqId))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.FaqId)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenBy(item => item.FaqId)),
            "contentrefid" => isFirst
                ? (desc
                    ? query.OrderByDescending(item => item.ContentRefId)
                    : query.OrderBy(item => item.ContentRefId))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.ContentRefId)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenBy(item =>
                        item.ContentRefId)),
            "createddate" => isFirst
                ? (desc ? query.OrderByDescending(item => item.CreatedDate) : query.OrderBy(item => item.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.CreatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.CreatedDate)),
            "updateddate" => isFirst
                ? (desc ? query.OrderByDescending(item => item.UpdatedDate) : query.OrderBy(item => item.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.UpdatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenBy(item => item.UpdatedDate)),
            "id" => isFirst
                ? (desc ? query.OrderByDescending(item => item.Id) : query.OrderBy(item => item.Id))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query)
                    .ThenByDescending(item => item.Id)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenBy(item => item.Id)),
            _ => isFirst
                ? query.OrderByDescending(item => item.UpdatedDate)
                : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItem>)query).ThenByDescending(item =>
                    item.UpdatedDate)
        };
    }
}
