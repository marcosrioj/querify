using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpace;

public sealed class QuestionSpacesGetQuestionSpaceQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionSpacesGetQuestionSpaceQuery, QuestionSpaceDto>
{
    public async Task<QuestionSpaceDto> Handle(QuestionSpacesGetQuestionSpaceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var entity = await dbContext.QuestionSpaces
            .Include(space => space.Questions)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                space =>
                    space.TenantId == tenantId &&
                    space.Id == request.Id &&
                    (space.Visibility == VisibilityScope.Public || space.Visibility == VisibilityScope.PublicIndexed),
                cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Question space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound)
            : entity.ToQuestionSpaceDto();
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}