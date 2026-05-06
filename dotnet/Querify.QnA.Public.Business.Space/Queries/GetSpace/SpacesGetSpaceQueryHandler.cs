using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Constants;
using Querify.Models.QnA.Dtos.Space;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Public.Business.Space.Queries.GetSpace;

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
            .AsNoTracking()
            .Where(space =>
                space.TenantId == tenantId &&
                space.Id == request.Id &&
                space.Visibility == VisibilityScope.Public &&
                space.Status == SpaceStatus.Active)
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
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound)
            : entity;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}
