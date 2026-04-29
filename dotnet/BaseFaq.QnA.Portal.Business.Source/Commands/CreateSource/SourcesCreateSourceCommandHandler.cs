using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Portal.Business.Source.Helpers;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Source.Commands.CreateSource;

public sealed class SourcesCreateSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesCreateSourceCommand, Guid>
{
    public async Task<Guid> Handle(SourcesCreateSourceCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = new Common.Persistence.QnADb.Entities.Source
        {
            TenantId = tenantId,
            Kind = request.Request.Kind,
            Locator = request.Request.Locator,
            Language = request.Request.Language,
            Checksum = SourceChecksum.FromLocator(request.Request.Locator),
            Visibility = request.Request.Visibility,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        dbContext.Sources.Add(entity);
        Apply(entity, request.Request, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(
        Common.Persistence.QnADb.Entities.Source entity,
        SourceCreateRequestDto request,
        string userId)
    {
        entity.Kind = request.Kind;
        entity.Locator = request.Locator;
        entity.Label = request.Label;
        entity.ContextNote = request.ContextNote;
        entity.ExternalId = request.ExternalId;
        entity.Language = request.Language;
        entity.MediaType = request.MediaType;
        entity.Checksum = SourceChecksum.FromLocator(request.Locator);
        entity.MetadataJson = request.MetadataJson;

        if (request.MarkVerified)
        {
            entity.LastVerifiedAtUtc = DateTime.UtcNow;
        }

        EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }

    private static void EnsureVisibilityAllowed(
        Common.Persistence.QnADb.Entities.Source entity,
        VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Kind is SourceKind.InternalNote)
            throw new ApiErrorException(
                "Internal notes cannot be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (entity.LastVerifiedAtUtc is null)
            throw new ApiErrorException(
                "Sources must be verified before public exposure.",
                (int)HttpStatusCode.UnprocessableEntity);
    }
}
