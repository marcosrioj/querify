using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Source;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Commands.CreateSource;

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
        var entity = new Common.Domain.Entities.Source
        {
            TenantId = tenantId,
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
        Common.Domain.Entities.Source entity,
        SourceCreateRequestDto request,
        string userId)
    {
        entity.Locator = request.Locator;
        entity.Label = request.Label;
        entity.ContextNote = request.ContextNote;
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
