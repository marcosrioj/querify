using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveTopic;

public sealed class QuestionSpacesRemoveTopicCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesRemoveTopicCommand>
{
    public async Task Handle(QuestionSpacesRemoveTopicCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var link = await dbContext.QuestionSpaceTopics
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.QuestionSpaceId == request.QuestionSpaceId &&
                    entity.TopicId == request.TopicId,
                cancellationToken);

        if (link is null)
        {
            throw new ApiErrorException(
                $"Question space topic link '{request.QuestionSpaceId}:{request.TopicId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.QuestionSpaceTopics.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
