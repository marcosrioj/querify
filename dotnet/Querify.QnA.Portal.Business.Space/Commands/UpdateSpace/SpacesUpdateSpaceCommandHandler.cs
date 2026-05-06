using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Space;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Portal.Business.Space.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Space.Commands.UpdateSpace;

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

        var slug = await SpaceSlugResolver.ResolveSlugAsync(
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
        Common.Domain.Entities.Space entity,
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
        SpaceRules.EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;

        entity.UpdatedBy = userId;
    }
}
