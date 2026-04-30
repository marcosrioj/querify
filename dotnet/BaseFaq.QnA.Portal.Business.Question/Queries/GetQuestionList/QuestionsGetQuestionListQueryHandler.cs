using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Mappings;
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

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var query = dbContext.Questions
            .Include(question => question.Space)
            .Include(question => question.Activities)
            .Where(question => question.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(question =>
                EF.Functions.ILike(question.Title, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(question.Summary ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(question.ContextNote ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(question.Space.Slug, $"%{request.Request.SearchText}%"));

        if (request.Request.SpaceId is not null)
            query = query.Where(question => question.SpaceId == request.Request.SpaceId);

        if (request.Request.SourceId is not null)
            query = query.Where(question => question.Sources.Any(link => link.SourceId == request.Request.SourceId.Value));

        if (request.Request.TagId is not null)
            query = query.Where(question => question.Tags.Any(link => link.TagId == request.Request.TagId.Value));

        if (request.Request.AcceptedAnswerId is not null)
            query = query.Where(question => question.AcceptedAnswerId == request.Request.AcceptedAnswerId);

        if (request.Request.Status is not null)
            query = query.Where(question => question.Status == request.Request.Status);

        if (request.Request.Visibility is not null)
            query = query.Where(question => question.Visibility == request.Request.Visibility);

        if (!string.IsNullOrWhiteSpace(request.Request.SpaceSlug))
            query = query.Where(question => question.Space.Slug == request.Request.SpaceSlug);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "lastactivityatutc" or "lastactivityatutc desc" => query.OrderByDescending(question =>
                question.LastActivityAtUtc ?? question.UpdatedDate ?? question.CreatedDate),
            "lastactivityatutc asc" => query.OrderBy(question =>
                question.LastActivityAtUtc ?? question.UpdatedDate ?? question.CreatedDate),
            "lastupdatedatutc" or "lastupdatedatutc asc" or "updateddate" or "updateddate asc" =>
                query.OrderBy(question => question.UpdatedDate ?? question.CreatedDate),
            "lastupdatedatutc desc" or "updateddate desc" => query.OrderByDescending(question =>
                question.UpdatedDate ?? question.CreatedDate),
            "sort" or "sort asc" => query.OrderBy(question => question.Sort),
            "sort desc" => query.OrderByDescending(question => question.Sort),
            "title" or "title asc" => query.OrderBy(question => question.Title),
            "title desc" => query.OrderByDescending(question => question.Title),
            "feedbackscore" or "feedbackscore asc" => query.OrderBy(question => question.FeedbackScore),
            "feedbackscore desc" => query.OrderByDescending(question => question.FeedbackScore),
            "aiconfidencescore" or "aiconfidencescore asc" => query.OrderBy(question => question.AiConfidenceScore),
            "aiconfidencescore desc" => query.OrderByDescending(question => question.AiConfidenceScore),
            "status" or "status asc" => query.OrderBy(question => question.Status),
            "status desc" => query.OrderByDescending(question => question.Status),
            _ => query.OrderByDescending(question =>
                question.LastActivityAtUtc ?? question.UpdatedDate ?? question.CreatedDate)
                .ThenBy(question => question.Title)
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
