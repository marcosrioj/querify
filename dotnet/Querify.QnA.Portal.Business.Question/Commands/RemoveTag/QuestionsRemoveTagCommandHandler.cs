using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Question.Commands.RemoveTag;

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