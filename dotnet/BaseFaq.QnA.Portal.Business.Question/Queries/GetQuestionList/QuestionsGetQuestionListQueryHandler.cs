using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestionList;

public sealed class QuestionsGetQuestionListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsGetQuestionListQuery, PagedResultDto<QuestionDto>>
{
    public async Task<PagedResultDto<QuestionDto>> Handle(QuestionsGetQuestionListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var query = dbContext.Questions
            .Include(question => question.Space)
            .Include(question => question.Activities)
            .Where(question => question.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(question =>
                EF.Functions.ILike(question.Title, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(question.Key, $"%{request.Request.SearchText}%"));

        if (request.Request.SpaceId is not null)
            query = query.Where(question => question.SpaceId == request.Request.SpaceId);

        if (request.Request.AcceptedAnswerId is not null)
            query = query.Where(question => question.AcceptedAnswerId == request.Request.AcceptedAnswerId);

        if (request.Request.DuplicateOfQuestionId is not null)
            query = query.Where(question => question.DuplicateOfQuestionId == request.Request.DuplicateOfQuestionId);

        if (request.Request.Status is not null)
            query = query.Where(question => question.Status == request.Request.Status);

        if (request.Request.Visibility is not null)
            query = query.Where(question => question.Visibility == request.Request.Visibility);

        if (!string.IsNullOrWhiteSpace(request.Request.SpaceKey))
            query = query.Where(question => question.Space.Key == request.Request.SpaceKey);

        if (!string.IsNullOrWhiteSpace(request.Request.ContextKey))
            query = query.Where(question => question.ContextKey == request.Request.ContextKey);

        if (!string.IsNullOrWhiteSpace(request.Request.Language))
            query = query.Where(question => question.Language == request.Request.Language);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "title desc" => query.OrderByDescending(question => question.Title),
            "resolvedatutc desc" => query.OrderByDescending(question => question.ResolvedAtUtc),
            "resolvedatutc" => query.OrderBy(question => question.ResolvedAtUtc),
            _ => query.OrderByDescending(question =>
                question.LastActivityAtUtc ?? question.UpdatedDate ?? question.CreatedDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QuestionDto>(totalCount, items.Select(entity => entity.ToQuestionDto()).ToList());
    }
}
