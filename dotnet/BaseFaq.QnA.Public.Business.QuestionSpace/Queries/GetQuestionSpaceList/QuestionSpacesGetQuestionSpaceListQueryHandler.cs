using BaseFaq.Models.Common.Dtos;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Queries;

public sealed class QuestionSpacesGetQuestionSpaceListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesGetQuestionSpaceListQuery, PagedResultDto<QuestionSpaceDto>>
{
    public async Task<PagedResultDto<QuestionSpaceDto>> Handle(QuestionSpacesGetQuestionSpaceListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        IQueryable<Common.Persistence.QnADb.Entities.QuestionSpace> query = dbContext.QuestionSpaces
            .Include(space => space.Questions)
            .Where(space =>
                space.TenantId == tenantId &&
                (space.Visibility == VisibilityScope.Public || space.Visibility == VisibilityScope.PublicIndexed));

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
        {
            query = query.Where(space =>
                EF.Functions.ILike(space.Name, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Key, $"%{request.Request.SearchText}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Request.ProductScope))
        {
            query = query.Where(space => space.ProductScope == request.Request.ProductScope);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.JourneyScope))
        {
            query = query.Where(space => space.JourneyScope == request.Request.JourneyScope);
        }

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "name desc" => query.OrderByDescending(space => space.Name),
            _ => query.OrderBy(space => space.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<QuestionSpaceDto>(
            totalCount,
            items.Select(
                    entity => new QuestionSpaceDto
                    {
                        Id = entity.Id,
                        TenantId = entity.TenantId,
                        Name = entity.Name,
                        Key = entity.Key,
                        Summary = entity.Summary,
                        DefaultLanguage = entity.DefaultLanguage,
                        Kind = entity.Kind,
                        Visibility = entity.Visibility,
                        ModerationPolicy = entity.ModerationPolicy,
                        SearchMarkupMode = entity.SearchMarkupMode,
                        ProductScope = entity.ProductScope,
                        JourneyScope = entity.JourneyScope,
                        AcceptsQuestions = entity.AcceptsQuestions,
                        AcceptsAnswers = entity.AcceptsAnswers,
                        RequiresQuestionReview = entity.RequiresQuestionReview,
                        RequiresAnswerReview = entity.RequiresAnswerReview,
                        PublishedAtUtc = entity.PublishedAtUtc,
                        LastValidatedAtUtc = entity.LastValidatedAtUtc,
                        QuestionCount = entity.Questions.Count
                    })
                .ToList());
    }
}
