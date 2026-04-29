using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Portal.Business.Space.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.UpdateSpace;

public sealed class SpacesUpdateSpaceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesUpdateSpaceCommand, Guid>
{
    public async Task<Guid> Handle(SpacesUpdateSpaceCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Spaces
            .Include(space => space.Tags)
            .ThenInclude(link => link.Tag)
            .Include(space => space.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(space => space.TenantId == tenantId && space.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var slug = await SpaceSlugHelper.ResolveSlugAsync(
            dbContext,
            tenantId,
            request.Id,
            request.Request.Slug,
            request.Request.Name,
            entity.Slug,
            cancellationToken);

        Apply(entity, request.Request, userId, slug);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(
        Common.Persistence.QnADb.Entities.Space entity,
        SpaceUpdateRequestDto request,
        string userId,
        string slug)
    {
        entity.Name = request.Name;
        entity.Slug = slug;
        entity.Language = request.Language;
        entity.Summary = request.Summary;
        entity.AcceptsQuestions = request.AcceptsQuestions;
        entity.AcceptsAnswers = request.AcceptsAnswers;
        entity.Status = request.Status;
        EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;

        entity.UpdatedBy = userId;
    }

    private static void EnsureVisibilityAllowed(
        Common.Persistence.QnADb.Entities.Space entity,
        VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not SpaceStatus.Active)
            throw new ApiErrorException(
                "Only active spaces can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);
    }
}
