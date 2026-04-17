using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Space.Queries.GetSpaceList;

public sealed class SpacesGetSpaceListQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesGetSpaceListQuery, PagedResultDto<SpaceDto>>
{
    public async Task<PagedResultDto<SpaceDto>> Handle(SpacesGetSpaceListQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var query = dbContext.Spaces
            .Include(space => space.Questions)
            .Include(space => space.Tags)
            .ThenInclude(link => link.Tag)
            .Include(space => space.Sources)
            .ThenInclude(link => link.Source)
            .Where(space => space.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(space =>
                EF.Functions.ILike(space.Name, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Key, $"%{request.Request.SearchText}%"));

        if (request.Request.Visibility is not null)
            query = query.Where(space => space.Visibility == request.Request.Visibility);

        if (request.Request.Kind is not null) query = query.Where(space => space.Kind == request.Request.Kind);

        if (!string.IsNullOrWhiteSpace(request.Request.ProductScope))
            query = query.Where(space => space.ProductScope == request.Request.ProductScope);

        if (!string.IsNullOrWhiteSpace(request.Request.JourneyScope))
            query = query.Where(space => space.JourneyScope == request.Request.JourneyScope);

        if (request.Request.AcceptsQuestions is not null)
            query = query.Where(space => space.AcceptsQuestions == request.Request.AcceptsQuestions);

        if (request.Request.AcceptsAnswers is not null)
            query = query.Where(space => space.AcceptsAnswers == request.Request.AcceptsAnswers);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "name desc" => query.OrderByDescending(space => space.Name),
            "key" => query.OrderBy(space => space.Key),
            "key desc" => query.OrderByDescending(space => space.Key),
            _ => query.OrderBy(space => space.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SpaceDto>(
            totalCount,
            items.Select(space => new SpaceDto
                {
                    Id = space.Id,
                    TenantId = space.TenantId,
                    Name = space.Name,
                    Key = space.Key,
                    Summary = space.Summary,
                    DefaultLanguage = space.DefaultLanguage,
                    Kind = space.Kind,
                    Visibility = space.Visibility,
                    ModerationPolicy = space.ModerationPolicy,
                    SearchMarkupMode = space.SearchMarkupMode,
                    ProductScope = space.ProductScope,
                    JourneyScope = space.JourneyScope,
                    AcceptsQuestions = space.AcceptsQuestions,
                    AcceptsAnswers = space.AcceptsAnswers,
                    RequiresQuestionReview = space.RequiresQuestionReview,
                    RequiresAnswerReview = space.RequiresAnswerReview,
                    PublishedAtUtc = space.PublishedAtUtc,
                    LastValidatedAtUtc = space.LastValidatedAtUtc,
                    QuestionCount = space.Questions.Count
                })
                .ToList());
    }
}