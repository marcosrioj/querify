using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswerList;

public class FaqItemAnswersGetFaqItemAnswerListQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqItemAnswersGetFaqItemAnswerListQuery, PagedResultDto<FaqItemAnswerDto>>
{
    public async Task<PagedResultDto<FaqItemAnswerDto>> Handle(
        FaqItemAnswersGetFaqItemAnswerListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.FaqItemAnswers.AsNoTracking();
        query = ApplyFilters(query, request.Request);
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
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
            .ToListAsync(cancellationToken);

        return new PagedResultDto<FaqItemAnswerDto>(totalCount, items);
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer> ApplyFilters(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer> query,
        FaqItemAnswerGetAllRequestDto request)
    {
        if (request.FaqItemId.HasValue)
        {
            query = query.Where(answer => answer.FaqItemId == request.FaqItemId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(answer => answer.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var pattern = $"%{request.SearchText.Trim()}%";
            query = query.Where(answer =>
                EF.Functions.ILike(answer.ShortAnswer, pattern) ||
                (answer.Answer != null && EF.Functions.ILike(answer.Answer, pattern)));
        }

        return query;
    }

    private static IQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer> ApplySorting(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer> query,
        string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query
                .OrderBy(answer => answer.Sort)
                .ThenByDescending(answer => answer.VoteScore)
                .ThenByDescending(answer => answer.UpdatedDate);
        }

        IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>? orderedQuery = null;
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

        return orderedQuery ?? query.OrderBy(answer => answer.Sort).ThenByDescending(answer => answer.VoteScore);
    }

    private static IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer> ApplyOrder(
        IQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer> query,
        string fieldName,
        bool desc,
        bool isFirst)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "shortanswer" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.ShortAnswer) : query.OrderBy(answer => answer.ShortAnswer))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.ShortAnswer)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.ShortAnswer)),
            "answer" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.Answer) : query.OrderBy(answer => answer.Answer))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.Answer)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.Answer)),
            "sort" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.Sort) : query.OrderBy(answer => answer.Sort))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.Sort)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.Sort)),
            "votescore" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.VoteScore) : query.OrderBy(answer => answer.VoteScore))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.VoteScore)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.VoteScore)),
            "isactive" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.IsActive) : query.OrderBy(answer => answer.IsActive))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.IsActive)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.IsActive)),
            "faqitemid" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.FaqItemId) : query.OrderBy(answer => answer.FaqItemId))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.FaqItemId)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.FaqItemId)),
            "createddate" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.CreatedDate) : query.OrderBy(answer => answer.CreatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.CreatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.CreatedDate)),
            "updateddate" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.UpdatedDate) : query.OrderBy(answer => answer.UpdatedDate))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.UpdatedDate)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.UpdatedDate)),
            "id" => isFirst
                ? (desc ? query.OrderByDescending(answer => answer.Id) : query.OrderBy(answer => answer.Id))
                : (desc
                    ? ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenByDescending(answer => answer.Id)
                    : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                    .ThenBy(answer => answer.Id)),
            _ => isFirst
                ? query.OrderBy(answer => answer.Sort).ThenByDescending(answer => answer.VoteScore)
                : ((IOrderedQueryable<Common.Persistence.FaqDb.Entities.FaqItemAnswer>)query)
                .ThenBy(answer => answer.Sort)
        };
    }
}
