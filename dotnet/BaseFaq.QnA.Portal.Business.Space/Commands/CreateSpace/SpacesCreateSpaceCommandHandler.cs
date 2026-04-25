using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using SpaceEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Space;

namespace BaseFaq.QnA.Portal.Business.Space.Commands.CreateSpace;

public sealed class SpacesCreateSpaceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesCreateSpaceCommand, Guid>
{
    public async Task<Guid> Handle(SpacesCreateSpaceCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();

        var entity = new SpaceEntity
        {
            TenantId = tenantId,
            Name = request.Request.Name,
            Key = request.Request.Key,
            DefaultLanguage = request.Request.DefaultLanguage,
            Kind = request.Request.Kind,
            Visibility = request.Request.Visibility,
            SearchMarkupMode = request.Request.SearchMarkupMode,
            AcceptsQuestions = request.Request.AcceptsQuestions,
            AcceptsAnswers = request.Request.AcceptsAnswers,
            CreatedBy = userId,
            UpdatedBy = userId
        };
        dbContext.Spaces.Add(entity);

        Apply(entity, request.Request, userId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(
        SpaceEntity entity,
        SpaceCreateRequestDto request,
        string userId)
    {
        entity.Name = request.Name;
        entity.Key = request.Key;
        entity.DefaultLanguage = request.DefaultLanguage;
        entity.Summary = request.Summary;
        entity.ProductScope = request.ProductScope;
        entity.JourneyScope = request.JourneyScope;
        entity.AcceptsQuestions = request.AcceptsQuestions;
        entity.AcceptsAnswers = request.AcceptsAnswers;
        entity.Kind = request.Kind;
        entity.Visibility = request.Visibility;
        entity.SearchMarkupMode = request.SearchMarkupMode;
        entity.PublishedAtUtc =
            request.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed
                ? DateTime.UtcNow
                : null;

        if (request.MarkValidated) entity.LastValidatedAtUtc = DateTime.UtcNow;

        entity.UpdatedBy = userId;
    }
}
