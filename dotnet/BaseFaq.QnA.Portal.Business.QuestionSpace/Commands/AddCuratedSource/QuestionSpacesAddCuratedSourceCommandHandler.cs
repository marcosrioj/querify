using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands;

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
            .Include(entity => entity.QuestionSpaceSources)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionSpaceId, cancellationToken);
        var source = await dbContext.KnowledgeSources
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.KnowledgeSourceId, cancellationToken);

        if (space is null)
        {
            throw new ApiErrorException(
                $"Question space '{request.Request.QuestionSpaceId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        if (source is null)
        {
            throw new ApiErrorException(
                $"Knowledge source '{request.Request.KnowledgeSourceId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        if (space.QuestionSpaceSources.All(link => link.KnowledgeSourceId != source.Id))
        {
            space.QuestionSpaceSources.Add(new Common.Persistence.QnADb.Entities.QuestionSpaceSource
            {
                TenantId = tenantId,
                QuestionSpaceId = space.Id,
                QuestionSpace = space,
                KnowledgeSourceId = source.Id,
                KnowledgeSource = source,
                CreatedBy = userId,
                UpdatedBy = userId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return space.QuestionSpaceSources.Single(link => link.KnowledgeSourceId == source.Id).Id;
    }
}
