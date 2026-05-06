using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Space.Commands.AddCuratedSource;

public sealed class SpacesAddCuratedSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesAddCuratedSourceCommand, Guid>
{
    public async Task<Guid> Handle(SpacesAddCuratedSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var space = await dbContext.Spaces
            .Include(entity => entity.Sources)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SpaceId,
                cancellationToken);
        var source = await dbContext.Sources
            .SingleOrDefaultAsync(
                entity => entity.TenantId == tenantId && entity.Id == request.Request.SourceId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException(
                $"Space '{request.Request.SpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (source is null)
            throw new ApiErrorException(
                $"Source '{request.Request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var link = SpaceRules.EnsureSourceLink(space, source, tenantId, userId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return link.Id;
    }
}
