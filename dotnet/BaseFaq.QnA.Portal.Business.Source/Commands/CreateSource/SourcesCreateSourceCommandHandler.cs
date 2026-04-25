using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
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
            Visibility = request.Request.Visibility,
            AllowsPublicCitation = request.Request.AllowsPublicCitation,
            AllowsPublicExcerpt = request.Request.AllowsPublicExcerpt,
            IsAuthoritative = request.Request.IsAuthoritative,
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
        entity.Scope = request.Scope;
        entity.SystemName = request.SystemName;
        entity.ExternalId = request.ExternalId;
        entity.Language = request.Language;
        entity.MediaType = request.MediaType;
        entity.Checksum = request.Checksum;
        entity.MetadataJson = request.MetadataJson;
        entity.CapturedAtUtc = request.CapturedAtUtc ?? entity.CapturedAtUtc;
        entity.Visibility = request.Visibility;
        entity.AllowsPublicCitation =
            request.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed &&
            request.AllowsPublicCitation;
        entity.AllowsPublicExcerpt =
            request.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed &&
            request.AllowsPublicExcerpt;

        if (request.MarkVerified)
        {
            entity.IsAuthoritative = request.IsAuthoritative;
            entity.LastVerifiedAtUtc = DateTime.UtcNow;
        }
        else if (request.IsAuthoritative != entity.IsAuthoritative && entity.LastVerifiedAtUtc is not null)
        {
            entity.IsAuthoritative = request.IsAuthoritative;
        }

        entity.UpdatedBy = userId;
    }
}