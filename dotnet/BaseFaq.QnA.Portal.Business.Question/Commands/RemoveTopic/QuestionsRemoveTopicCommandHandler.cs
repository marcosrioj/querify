using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands;

public sealed class QuestionsRemoveTopicCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsRemoveTopicCommand>
{
    public async Task Handle(QuestionsRemoveTopicCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var link = await dbContext.QuestionTopics
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.QuestionId == request.QuestionId &&
                    entity.TopicId == request.TopicId,
                cancellationToken);

        if (link is null)
        {
            throw new ApiErrorException(
                $"Question topic link '{request.QuestionId}:{request.TopicId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.QuestionTopics.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
