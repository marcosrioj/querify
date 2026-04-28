using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
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
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
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
                EF.Functions.ILike(space.Slug, $"%{request.Request.SearchText}%"));

        if (request.Request.Visibility is not null)
            query = query.Where(space => space.Visibility == request.Request.Visibility);

        if (request.Request.Kind is not null) query = query.Where(space => space.Kind == request.Request.Kind);

        if (request.Request.AcceptsQuestions is not null)
            query = query.Where(space => space.AcceptsQuestions == request.Request.AcceptsQuestions);

        if (request.Request.AcceptsAnswers is not null)
            query = query.Where(space => space.AcceptsAnswers == request.Request.AcceptsAnswers);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "name desc" => query.OrderByDescending(space => space.Name),
            "slug" => query.OrderBy(space => space.Slug),
            "slug desc" => query.OrderByDescending(space => space.Slug),
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
                    Slug = space.Slug,
                    Summary = space.Summary,
                    Language = space.Language,
                    Kind = space.Kind,
                    Visibility = space.Visibility,
                    AcceptsQuestions = space.AcceptsQuestions,
                    AcceptsAnswers = space.AcceptsAnswers,
                    PublishedAtUtc = space.PublishedAtUtc,
                    QuestionCount = space.Questions.Count
                })
                .ToList());
    }
}
