using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries.GetKnowledgeSource;

public sealed class KnowledgeSourcesGetKnowledgeSourceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<KnowledgeSourcesGetKnowledgeSourceQuery, KnowledgeSourceDto>
{
    public async Task<KnowledgeSourceDto> Handle(KnowledgeSourcesGetKnowledgeSourceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.KnowledgeSources.AsNoTracking()
            .SingleOrDefaultAsync(source => source.TenantId == tenantId && source.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ApiErrorException(
                $"Knowledge source '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound)
            : new KnowledgeSourceDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Kind = entity.Kind,
                Locator = entity.Locator,
                Label = entity.Label,
                Scope = entity.Scope,
                SystemName = entity.SystemName,
                ExternalId = entity.ExternalId,
                Language = entity.Language,
                MediaType = entity.MediaType,
                Checksum = entity.Checksum,
                MetadataJson = entity.MetadataJson,
                Visibility = entity.Visibility,
                AllowsPublicCitation = entity.AllowsPublicCitation,
                AllowsPublicExcerpt = entity.AllowsPublicExcerpt,
                IsAuthoritative = entity.IsAuthoritative,
                CapturedAtUtc = entity.CapturedAtUtc,
                LastVerifiedAtUtc = entity.LastVerifiedAtUtc
            };
    }
}