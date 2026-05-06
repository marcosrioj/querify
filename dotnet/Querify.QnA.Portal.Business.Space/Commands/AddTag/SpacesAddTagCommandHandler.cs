using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Space.Commands.AddTag;

public sealed class SpacesAddTagCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesAddTagCommand, Guid>
{
    public async Task<Guid> Handle(SpacesAddTagCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var space = await dbContext.Spaces
            .Include(entity => entity.Tags)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SpaceId,
                cancellationToken);
        var tag = await dbContext.Tags
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.TagId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException(
                $"Space '{request.Request.SpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (tag is null)
            throw new ApiErrorException($"Tag '{request.Request.TagId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var link = SpaceRules.EnsureTagLink(space, tag, tenantId, userId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return link.Id;
    }
}
