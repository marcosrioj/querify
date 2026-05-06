using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Space.Commands.RemoveTag;

public sealed class SpacesRemoveTagCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesRemoveTagCommand>
{
    public async Task Handle(SpacesRemoveTagCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var link = await dbContext.SpaceTags
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.SpaceId == request.SpaceId &&
                    entity.TagId == request.TagId,
                cancellationToken);

        if (link is null)
            throw new ApiErrorException(
                $"Space tag link '{request.SpaceId}:{request.TagId}' was not found.",
                (int)HttpStatusCode.NotFound);

        dbContext.SpaceTags.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}