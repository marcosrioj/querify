using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RemoveTag;

public sealed class QuestionsRemoveTagCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsRemoveTagCommand>
{
    public async Task Handle(QuestionsRemoveTagCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var link = await dbContext.QuestionTags
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.QuestionId == request.QuestionId &&
                    entity.TagId == request.TagId,
                cancellationToken);

        if (link is null)
            throw new ApiErrorException(
                $"Question tag link '{request.QuestionId}:{request.TagId}' was not found.",
                (int)HttpStatusCode.NotFound);

        dbContext.QuestionTags.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}