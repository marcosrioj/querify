using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.Entities;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.QnA.Portal.Business.Source.Commands.CompleteUpload;

public sealed class SourcesCompleteUploadCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IObjectStorage objectStorage,
    IOptions<SourceUploadOptions> uploadOptions)
    : IRequestHandler<SourcesCompleteUploadCommand, Guid>
{
    public async Task<Guid> Handle(SourcesCompleteUploadCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Sources
            .SingleOrDefaultAsync(source => source.TenantId == tenantId && source.Id == request.SourceId,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException(
                $"Source '{request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (entity.UploadStatus is not SourceUploadStatus.Pending)
            throw new ApiErrorException(
                "Upload was already finalized.",
                (int)HttpStatusCode.Conflict);

        if (string.IsNullOrWhiteSpace(entity.StorageKey))
            throw new ApiErrorException(
                "Upload intent storage key is missing.",
                (int)HttpStatusCode.UnprocessableEntity);

        var head = await objectStorage.HeadAsync(entity.StorageKey, cancellationToken);
        if (head is null)
            throw new ApiErrorException(
                "Upload was not received.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (head.SizeBytes != entity.SizeBytes ||
            head.SizeBytes > uploadOptions.Value.MaxUploadBytes)
        {
            await RejectUploadedObjectAsync(entity, userId, cancellationToken);
            throw new ApiErrorException(
                "Uploaded object size does not match the upload intent.",
                (int)HttpStatusCode.UnprocessableEntity);
        }

        var headContentType = SourceRules.NormalizeContentType(head.ContentType);
        var declaredContentType = SourceRules.NormalizeContentType(entity.MediaType);
        if (headContentType is null ||
            !SourceRules.IsUploadContentTypeAllowed(entity.Kind, headContentType, uploadOptions.Value.AllowedContentTypes) ||
            !StringComparer.OrdinalIgnoreCase.Equals(headContentType, declaredContentType))
        {
            await RejectUploadedObjectAsync(entity, userId, cancellationToken);
            throw new ApiErrorException(
                "Uploaded object content type does not match the upload intent.",
                (int)HttpStatusCode.UnprocessableEntity);
        }

        entity.SizeBytes = head.SizeBytes;
        entity.MediaType = headContentType;
        entity.UploadChecksum = request.ClientChecksum;
        entity.UploadStatus = SourceUploadStatus.Uploaded;
        entity.UpdatedBy = userId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private async Task RejectUploadedObjectAsync(
        Common.Domain.Entities.Source entity,
        string userId,
        CancellationToken cancellationToken)
    {
        await objectStorage.DeleteAsync(entity.StorageKey!, cancellationToken);
        entity.UploadStatus = SourceUploadStatus.Failed;
        entity.UploadChecksum = null;
        entity.UpdatedBy = userId;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
