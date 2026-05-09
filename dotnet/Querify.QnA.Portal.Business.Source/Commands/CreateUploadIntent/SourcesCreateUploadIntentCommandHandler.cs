using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.Extensions.Options;

namespace Querify.QnA.Portal.Business.Source.Commands.CreateUploadIntent;

public sealed class SourcesCreateUploadIntentCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IObjectStorage objectStorage,
    IOptions<SourceUploadOptions> uploadOptions)
    : IRequestHandler<SourcesCreateUploadIntentCommand, SourceUploadIntentResponseDto>
{
    public async Task<SourceUploadIntentResponseDto> Handle(SourcesCreateUploadIntentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Dto);

        ValidateRequest(request.Dto);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var sourceId = Guid.NewGuid();
        var contentType = SourceRules.NormalizeContentType(request.Dto.ContentType)!;
        var stagingKey = SourceStorageKey.BuildStagingKey(tenantId, sourceId, request.Dto.FileName);
        var locator = SourceStorageKey.ToLocator(stagingKey);
        var entity = new Common.Domain.Entities.Source
        {
            Id = sourceId,
            TenantId = tenantId,
            Locator = locator,
            StorageKey = stagingKey,
            Label = request.Dto.Label,
            ContextNote = request.Dto.ContextNote,
            ExternalId = request.Dto.ExternalId,
            MetadataJson = request.Dto.MetadataJson,
            Language = request.Dto.Language,
            MediaType = contentType,
            SizeBytes = request.Dto.SizeBytes,
            Checksum = SourceChecksum.FromLocator(stagingKey),
            UploadStatus = SourceUploadStatus.Pending,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        dbContext.Sources.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var presign = await objectStorage.PresignPutAsync(
            stagingKey,
            contentType,
            request.Dto.SizeBytes,
            cancellationToken);

        return new SourceUploadIntentResponseDto
        {
            SourceId = sourceId,
            UploadUrl = presign.Url.ToString(),
            RequiredHeaders = presign.RequiredHeaders,
            StorageKey = stagingKey,
            ExpiresAtUtc = presign.ExpiresAtUtc
        };
    }

    private void ValidateRequest(SourceUploadIntentRequestDto request)
    {
        var options = uploadOptions.Value;

        if (request.SizeBytes <= 0 || request.SizeBytes > options.MaxUploadBytes)
            throw new ApiErrorException(
                "Upload size exceeds the allowed limit.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (!SourceRules.IsUploadContentTypeAllowed(request.ContentType, options.AllowedContentTypes) ||
            !SourceRules.IsUploadFileNameAllowed(request.FileName, request.ContentType))
            throw new ApiErrorException(
                "Upload content type is not allowed for sources.",
                (int)HttpStatusCode.UnprocessableEntity);
    }
}
