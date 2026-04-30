using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using BaseFaq.QnA.Common.Persistence.QnADb.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswerList;

public sealed class AnswersGetAnswerListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersGetAnswerListQuery, PagedResultDto<AnswerDto>>
{
    public async Task<PagedResultDto<AnswerDto>> Handle(AnswersGetAnswerListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var query = dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .Where(answer => answer.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(answer =>
                EF.Functions.ILike(answer.Headline, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.Body ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.ContextNote ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.AuthorLabel ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.Question.Title, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(answer.Question.Space.Slug, $"%{request.Request.SearchText}%"));

        if (request.Request.SpaceId is not null)
            query = query.Where(answer => answer.Question.SpaceId == request.Request.SpaceId);

        if (request.Request.SourceId is not null)
            query = query.Where(answer => answer.Sources.Any(link => link.SourceId == request.Request.SourceId.Value));

        if (request.Request.QuestionId is not null)
            query = query.Where(answer => answer.QuestionId == request.Request.QuestionId);

        if (request.Request.Status is not null) query = query.Where(answer => answer.Status == request.Request.Status);

        if (request.Request.Visibility is not null)
            query = query.Where(answer => answer.Visibility == request.Request.Visibility);

        if (request.Request.IsAccepted is not null)
            query = request.Request.IsAccepted.Value
                ? query.Where(answer => answer.Question != null && answer.Question.AcceptedAnswerId == answer.Id)
                : query.Where(answer => answer.Question == null || answer.Question.AcceptedAnswerId != answer.Id);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "lastupdatedatutc" or "lastupdatedatutc desc" or "updateddate" or "updateddate desc" =>
                query.OrderByDescending(answer => answer.UpdatedDate ?? answer.CreatedDate),
            "lastupdatedatutc asc" or "updateddate asc" => query.OrderBy(answer =>
                answer.UpdatedDate ?? answer.CreatedDate),
            "headline" or "headline asc" => query.OrderBy(answer => answer.Headline),
            "headline desc" => query.OrderByDescending(answer => answer.Headline),
            "score" or "score asc" => query.OrderBy(answer => answer.Score),
            "score desc" => query.OrderByDescending(answer => answer.Score),
            "sort" or "sort asc" => query.OrderBy(answer => answer.Sort),
            "sort desc" => query.OrderByDescending(answer => answer.Sort),
            "aiconfidencescore" or "aiconfidencescore asc" => query.OrderBy(answer => answer.AiConfidenceScore),
            "aiconfidencescore desc" => query.OrderByDescending(answer => answer.AiConfidenceScore),
            "status" or "status asc" => query.OrderBy(answer => answer.Status),
            "status desc" => query.OrderByDescending(answer => answer.Status),
            "kind" or "kind asc" => query.OrderBy(answer => answer.Kind),
            "kind desc" => query.OrderByDescending(answer => answer.Kind),
            _ => query.OrderByDescending(answer => answer.UpdatedDate ?? answer.CreatedDate)
                .ThenByDescending(answer => answer.Question != null && answer.Question.AcceptedAnswerId == answer.Id)
                .ThenBy(answer => answer.Headline)
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
                IEnumerable<Activity> questionActivity = answer.Question?.Activities ?? [];
                return answer.ToPortalAnswerDto(questionActivity, answer.Question?.AcceptedAnswerId);
            }).ToList());
    }
}
