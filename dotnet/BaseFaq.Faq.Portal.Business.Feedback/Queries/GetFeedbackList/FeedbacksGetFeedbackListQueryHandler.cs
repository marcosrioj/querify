using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Feedback;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedbackList;

public class FeedbacksGetFeedbackListQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FeedbacksGetFeedbackListQuery, PagedResultDto<FeedbackDto>>
{
    public async Task<PagedResultDto<FeedbackDto>> Handle(
        FeedbacksGetFeedbackListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.Feedbacks.AsNoTracking();
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
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
            .ToListAsync(cancellationToken);

        return new PagedResultDto<FeedbackDto>(totalCount, items);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.Feedback> ApplySorting(
        IQueryable<Common.Persistence.FaqDb.Entities.Feedback> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(feedback => feedback.UpdatedDate);
        }

        IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>? orderedQuery = null;
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

        return orderedQuery ?? query.OrderByDescending(feedback => feedback.UpdatedDate);
    }

    private static IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback> ApplyOrder(
        IQueryable<Common.Persistence.FaqDb.Entities.Feedback> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "like" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.Like) : query.OrderBy(feedback => feedback.Like))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.Like)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenBy(feedback => feedback.Like)),
            "userprint" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.UserPrint) : query.OrderBy(feedback => feedback.UserPrint))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.UserPrint)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query)
                    .ThenBy(feedback => feedback.UserPrint)),
            "ip" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.Ip) : query.OrderBy(feedback => feedback.Ip))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.Ip)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenBy(feedback => feedback.Ip)),
            "useragent" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.UserAgent) : query.OrderBy(feedback => feedback.UserAgent))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.UserAgent)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query)
                    .ThenBy(feedback => feedback.UserAgent)),
            "unlikereason" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.UnLikeReason) : query.OrderBy(feedback => feedback.UnLikeReason))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.UnLikeReason)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenBy(feedback =>
                        feedback.UnLikeReason)),
            "faqitemid" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.FaqItemId) : query.OrderBy(feedback => feedback.FaqItemId))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.FaqItemId)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query)
                    .ThenBy(feedback => feedback.FaqItemId)),
            "createddate" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.CreatedDate) : query.OrderBy(feedback => feedback.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.CreatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query)
                    .ThenBy(feedback => feedback.CreatedDate)),
            "updateddate" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.UpdatedDate) : query.OrderBy(feedback => feedback.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.UpdatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query)
                    .ThenBy(feedback => feedback.UpdatedDate)),
            "id" => isFirst
                ? (desc ? query.OrderByDescending(feedback => feedback.Id) : query.OrderBy(feedback => feedback.Id))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                        feedback.Id)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenBy(feedback => feedback.Id)),
            _ => isFirst
                ? query.OrderByDescending(feedback => feedback.UpdatedDate)
                : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.Feedback>)query).ThenByDescending(feedback =>
                    feedback.UpdatedDate)
        };
    }
}