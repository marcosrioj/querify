using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Constants;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Space;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Public.Business.Space.Queries.GetSpaceList;

public sealed class SpacesGetSpaceListQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<SpacesGetSpaceListQuery, PagedResultDto<SpaceDto>>
{
    public async Task<PagedResultDto<SpaceDto>> Handle(SpacesGetSpaceListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var query = dbContext.Spaces
            .AsNoTracking()
            .Where(space =>
                space.TenantId == tenantId &&
                space.Visibility == VisibilityScope.Public &&
                space.Status == SpaceStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(space =>
                EF.Functions.ILike(space.Name, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Slug, $"%{request.Request.SearchText}%"));

        query = request.Request.Sorting?.Trim().ToLowerInvariant() switch
        {
            "name desc" => query.OrderByDescending(space => space.Name),
            _ => query.OrderBy(space => space.Name)
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

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}
