using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpaceByKey;

public sealed class QuestionSpacesGetQuestionSpaceByKeyQueryHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionSpacesGetQuestionSpaceByKeyQuery, QuestionSpaceDto>
{
    public async Task<QuestionSpaceDto> Handle(QuestionSpacesGetQuestionSpaceByKeyQuery request, CancellationToken cancellationToken)
    {
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var entity = await dbContext.QuestionSpaces
            .Include(space => space.Questions)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                space =>
                    space.TenantId == tenantId &&
                    space.Key == request.Key &&
                    (space.Visibility == VisibilityScope.Public || space.Visibility == VisibilityScope.PublicIndexed),
                cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Question space '{request.Key}' was not found.", errorCode: (int)HttpStatusCode.NotFound)
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
