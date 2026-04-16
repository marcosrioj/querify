using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands;

public sealed class QuestionSpacesRemoveCuratedSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesRemoveCuratedSourceCommand>
{
    public async Task Handle(QuestionSpacesRemoveCuratedSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var link = await dbContext.QuestionSpaceSources
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.QuestionSpaceId == request.QuestionSpaceId &&
                    entity.KnowledgeSourceId == request.KnowledgeSourceId,
                cancellationToken);

        if (link is null)
        {
            throw new ApiErrorException(
                $"Question space source link '{request.QuestionSpaceId}:{request.KnowledgeSourceId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.QuestionSpaceSources.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
