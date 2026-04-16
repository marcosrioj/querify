using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.UpdateKnowledgeSource;

public sealed class KnowledgeSourcesUpdateKnowledgeSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<KnowledgeSourcesUpdateKnowledgeSourceCommand, Guid>
{
    public async Task<Guid> Handle(KnowledgeSourcesUpdateKnowledgeSourceCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.KnowledgeSources
            .SingleOrDefaultAsync(source => source.TenantId == tenantId && source.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException(
                $"Knowledge source '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound);

        Apply(entity, request.Request, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(
        Common.Persistence.QnADb.Entities.KnowledgeSource entity,
        KnowledgeSourceUpdateRequestDto request,
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