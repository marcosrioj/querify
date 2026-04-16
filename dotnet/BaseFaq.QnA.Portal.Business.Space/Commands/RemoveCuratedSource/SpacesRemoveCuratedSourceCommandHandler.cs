using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.RemoveCuratedSource;

public sealed class SpacesRemoveCuratedSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesRemoveCuratedSourceCommand>
{
    public async Task Handle(SpacesRemoveCuratedSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var link = await dbContext.SpaceSources
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.SpaceId == request.SpaceId &&
                    entity.SourceId == request.SourceId,
                cancellationToken);

        if (link is null)
            throw new ApiErrorException(
                $"Space source link '{request.SpaceId}:{request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        dbContext.SpaceSources.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}