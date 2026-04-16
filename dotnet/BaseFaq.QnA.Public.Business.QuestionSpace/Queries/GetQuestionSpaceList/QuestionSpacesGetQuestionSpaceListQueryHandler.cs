using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpaceList;

public sealed class QuestionSpacesGetQuestionSpaceListQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionSpacesGetQuestionSpaceListQuery, PagedResultDto<QuestionSpaceDto>>
{
    public async Task<PagedResultDto<QuestionSpaceDto>> Handle(QuestionSpacesGetQuestionSpaceListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var query = dbContext.QuestionSpaces
            .Include(space => space.Questions)
            .Where(space =>
                space.TenantId == tenantId &&
                (space.Visibility == VisibilityScope.Public || space.Visibility == VisibilityScope.PublicIndexed));

        if (!string.IsNullOrWhiteSpace(request.Request.SearchText))
            query = query.Where(space =>
                EF.Functions.ILike(space.Name, $"%{request.Request.SearchText}%") ||
                EF.Functions.ILike(space.Key, $"%{request.Request.SearchText}%"));

        if (!string.IsNullOrWhiteSpace(request.Request.ProductScope))
            query = query.Where(space => space.ProductScope == request.Request.ProductScope);

        if (!string.IsNullOrWhiteSpace(request.Request.JourneyScope))
            query = query.Where(space => space.JourneyScope == request.Request.JourneyScope);

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
            items.Select(entity => entity.ToQuestionSpaceDto()).ToList());
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}