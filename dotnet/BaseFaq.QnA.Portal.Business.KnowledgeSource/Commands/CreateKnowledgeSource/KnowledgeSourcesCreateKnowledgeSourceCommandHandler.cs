using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.CreateKnowledgeSource;

public sealed class KnowledgeSourcesCreateKnowledgeSourceCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<KnowledgeSourcesCreateKnowledgeSourceCommand, Guid>
{
    public async Task<Guid> Handle(KnowledgeSourcesCreateKnowledgeSourceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = new Common.Persistence.QnADb.Entities.KnowledgeSource
        {
            TenantId = tenantId,
            Kind = request.Request.Kind,
            Locator = request.Request.Locator,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        dbContext.KnowledgeSources.Add(entity);
        Apply(entity, request.Request, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(
        Common.Persistence.QnADb.Entities.KnowledgeSource entity,
        KnowledgeSourceCreateRequestDto request,
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
            request.Visibility is BaseFaq.Models.QnA.Enums.VisibilityScope.Public or BaseFaq.Models.QnA.Enums.VisibilityScope.PublicIndexed &&
            request.AllowsPublicCitation;
        entity.AllowsPublicExcerpt =
            request.Visibility is BaseFaq.Models.QnA.Enums.VisibilityScope.Public or BaseFaq.Models.QnA.Enums.VisibilityScope.PublicIndexed &&
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
