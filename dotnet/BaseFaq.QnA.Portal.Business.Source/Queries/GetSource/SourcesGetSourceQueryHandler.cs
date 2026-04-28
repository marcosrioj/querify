using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Source.Queries.GetSource;

public sealed class SourcesGetSourceQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesGetSourceQuery, SourceDto>
{
    public async Task<SourceDto> Handle(SourcesGetSourceQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Sources.AsNoTracking()
            .SingleOrDefaultAsync(source => source.TenantId == tenantId && source.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ApiErrorException(
                $"Source '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound)
            : new SourceDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Kind = entity.Kind,
                Locator = entity.Locator,
                Label = entity.Label,
                ContextNote = entity.ContextNote,
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
