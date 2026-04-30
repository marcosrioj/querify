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
            .AsNoTracking()
            .Where(space => space.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(space =>
                EF.Functions.ILike(space.Name, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Slug, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Summary ?? string.Empty, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Language, $"%{request.Request.SearchText}%"));

        if (request.Request.Visibility is not null)
            query = query.Where(space => space.Visibility == request.Request.Visibility);

        if (request.Request.Status is not null) query = query.Where(space => space.Status == request.Request.Status);

        if (request.Request.AcceptsQuestions is not null)
            query = query.Where(space => space.AcceptsQuestions == request.Request.AcceptsQuestions);

        if (request.Request.AcceptsAnswers is not null)
            query = query.Where(space => space.AcceptsAnswers == request.Request.AcceptsAnswers);

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "name" or "name asc" => query.OrderBy(space => space.Name),
            "name desc" => query.OrderByDescending(space => space.Name),
            "slug" or "slug asc" => query.OrderBy(space => space.Slug),
            "slug desc" => query.OrderByDescending(space => space.Slug),
            "questioncount" or "questioncount asc" => query.OrderBy(space => space.Questions.Count),
            "questioncount desc" => query.OrderByDescending(space => space.Questions.Count),
            "status" or "status asc" => query.OrderBy(space => space.Status),
            "status desc" => query.OrderByDescending(space => space.Status),
            "visibility" or "visibility asc" => query.OrderBy(space => space.Visibility),
            "visibility desc" => query.OrderByDescending(space => space.Visibility),
            "lastupdatedatutc" or "lastupdatedatutc asc" or "updateddate" or "updateddate asc" =>
                query.OrderBy(space => space.UpdatedDate ?? space.CreatedDate),
            _ => query.OrderByDescending(space => space.UpdatedDate ?? space.CreatedDate)
                .ThenBy(space => space.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(space => new SpaceDto
            {
                Id = space.Id,
                TenantId = space.TenantId,
                Name = space.Name,
                Slug = space.Slug,
                Summary = space.Summary,
                Language = space.Language,
                Status = space.Status,
                Visibility = space.Visibility,
                AcceptsQuestions = space.AcceptsQuestions,
                AcceptsAnswers = space.AcceptsAnswers,
                QuestionCount = space.Questions.Count,
                LastUpdatedAtUtc = space.UpdatedDate ?? space.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SpaceDto>(
            totalCount,
            items);
    }
}
