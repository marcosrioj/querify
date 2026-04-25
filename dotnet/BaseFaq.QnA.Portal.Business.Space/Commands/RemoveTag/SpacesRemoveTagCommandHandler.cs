using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.RemoveTag;

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