using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.QnA.Common.Domain.BusinessRules.Sources;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Source.Commands.UpdateSource;

public sealed class SourcesUpdateSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SourcesUpdateSourceCommand, Guid>
{
    public async Task<Guid> Handle(SourcesUpdateSourceCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Sources
            .SingleOrDefaultAsync(source => source.TenantId == tenantId && source.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException(
                $"Source '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound);

        Apply(entity, request.Request, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(
        Common.Domain.Entities.Source entity,
        SourceUpdateRequestDto request,
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

        SourceRules.EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }
}
