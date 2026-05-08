using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Space;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Portal.Business.Space.Infrastructure;
using MediatR;

namespace Querify.QnA.Portal.Business.Space.Commands.CreateSpace;

public sealed class SpacesCreateSpaceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesCreateSpaceCommand, Guid>
{
    public async Task<Guid> Handle(SpacesCreateSpaceCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var slug = await SpaceSlugResolver.ResolveSlugAsync(
            dbContext,
            tenantId,
            spaceId: null,
            request.Request.Slug,
            request.Request.Name,
            currentSlug: null,
            cancellationToken);

        var entity = new Common.Domain.Entities.Space
        {
            TenantId = tenantId,
            Name = request.Request.Name,
            Slug = slug,
            Language = request.Request.Language,
            Status = request.Request.Status,
            Visibility = request.Request.Visibility,
            AcceptsQuestions = request.Request.AcceptsQuestions,
            AcceptsAnswers = request.Request.AcceptsAnswers,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        dbContext.Spaces.Add(entity);

        Apply(entity, request.Request, userId, slug);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(
        Common.Domain.Entities.Space entity,
        SpaceCreateRequestDto request,
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
