using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Space.Queries.GetSpace;

public sealed class SpacesGetSpaceQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<SpacesGetSpaceQuery, SpaceDto>
{
    public async Task<SpaceDto> Handle(SpacesGetSpaceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var entity = await dbContext.Spaces
            .Include(space => space.Questions)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                space =>
                    space.TenantId == tenantId &&
                    space.Id == request.Id &&
                    (space.Visibility == VisibilityScope.Public || space.Visibility == VisibilityScope.PublicIndexed),
                cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound)
            : entity.ToSpaceDto();
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}