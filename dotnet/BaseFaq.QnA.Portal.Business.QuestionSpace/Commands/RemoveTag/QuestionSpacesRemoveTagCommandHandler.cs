using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveTag;

public sealed class QuestionSpacesRemoveTagCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesRemoveTagCommand>
{
    public async Task Handle(QuestionSpacesRemoveTagCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var link = await dbContext.QuestionSpaceTags
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.QuestionSpaceId == request.QuestionSpaceId &&
                    entity.TagId == request.TagId,
                cancellationToken);

        if (link is null)
            throw new ApiErrorException(
                $"Question space tag link '{request.QuestionSpaceId}:{request.TagId}' was not found.",
                (int)HttpStatusCode.NotFound);

        dbContext.QuestionSpaceTags.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}