using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Common.Infrastructure.Storage.Options;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.QnA.Portal.Business.Source.Queries.GetDownloadUrl;

public sealed class SourcesGetDownloadUrlQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IObjectStorage objectStorage,
    IOptions<ObjectStorageOptions> objectStorageOptions)
    : IRequestHandler<SourcesGetDownloadUrlQuery, SourceDownloadUrlDto>
{
    public async Task<SourceDownloadUrlDto> Handle(SourcesGetDownloadUrlQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var source = await dbContext.Sources.AsNoTracking()
            .Where(source => source.TenantId == tenantId && source.Id == request.Id)
            .Select(source => new SourceDownloadReadModel(
                source.StorageKey,
                source.Visibility,
                source.UploadStatus,
                source.TenantId))
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
            throw new ApiErrorException(
                $"Source '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound);

        SourceRules.EnsureStorageKeyIsDownloadable(source.StorageKey, source.UploadStatus);

        var ttl = TimeSpan.FromMinutes(objectStorageOptions.Value.DownloadPresignTtlMinutes);
        var expiresAtUtc = DateTime.UtcNow.Add(ttl);
        var url = await objectStorage.PresignGetAsync(source.StorageKey!, ttl, cancellationToken);
        return new SourceDownloadUrlDto
        {
            Url = url.ToString(),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private sealed record SourceDownloadReadModel(
        string? StorageKey,
        VisibilityScope Visibility,
        SourceUploadStatus UploadStatus,
        Guid TenantId);
}
