using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Activity;

namespace BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswerList;

public sealed class AnswersGetAnswerListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersGetAnswerListQuery, PagedResultDto<AnswerDto>>
{
    public async Task<PagedResultDto<AnswerDto>> Handle(AnswersGetAnswerListQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var query = dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .Where(answer => answer.TenantId == tenantId);

        if (request.Request.QuestionId is not null)
            query = query.Where(answer => answer.QuestionId == request.Request.QuestionId);

        if (request.Request.Status is not null) query = query.Where(answer => answer.Status == request.Request.Status);

        if (request.Request.Visibility is not null)
            query = query.Where(answer => answer.Visibility == request.Request.Visibility);

        if (!string.IsNullOrWhiteSpace(request.Request.ContextKey))
            query = query.Where(answer => answer.ContextKey == request.Request.ContextKey);

        if (request.Request.IsAccepted is not null)
            query = request.Request.IsAccepted.Value
                ? query.Where(answer => answer.Question != null && answer.Question.AcceptedAnswerId == answer.Id)
                : query.Where(answer => answer.Question == null || answer.Question.AcceptedAnswerId != answer.Id);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "headline desc" => query.OrderByDescending(answer => answer.Headline),
            "rank" => query.OrderBy(answer => answer.Rank),
            "rank desc" => query.OrderByDescending(answer => answer.Rank),
            _ => query.OrderByDescending(answer => answer.Question != null && answer.Question.AcceptedAnswerId == answer.Id)
                .ThenByDescending(answer => answer.Rank)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AnswerDto>(
            totalCount,
            items.Select(answer =>
            {
                IEnumerable<ActivityEntity> questionActivity = answer.Question?.Activities ?? [];
                return answer.ToPortalAnswerDto(questionActivity, answer.Question?.AcceptedAnswerId);
            }).ToList());
    }
}
