using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddCuratedSource;

public sealed class QuestionSpacesAddCuratedSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesAddCuratedSourceCommand, Guid>
{
    public async Task<Guid> Handle(QuestionSpacesAddCuratedSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var space = await dbContext.QuestionSpaces
            .Include(entity => entity.Sources)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionSpaceId,
                cancellationToken);
        var source = await dbContext.KnowledgeSources
            .SingleOrDefaultAsync(
                entity => entity.TenantId == tenantId && entity.Id == request.Request.KnowledgeSourceId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException(
                $"Question space '{request.Request.QuestionSpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (source is null)
            throw new ApiErrorException(
                $"Knowledge source '{request.Request.KnowledgeSourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (space.Sources.All(link => link.KnowledgeSourceId != source.Id))
            space.Sources.Add(new QuestionSpaceSource
            {
                TenantId = tenantId,
                QuestionSpaceId = space.Id,
                QuestionSpace = space,
                KnowledgeSourceId = source.Id,
                KnowledgeSource = source,
                CreatedBy = userId,
                UpdatedBy = userId
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        return space.Sources.Single(link => link.KnowledgeSourceId == source.Id).Id;
    }
}
