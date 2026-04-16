using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RemoveSource;

public sealed class QuestionsRemoveSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsRemoveSourceCommand>
{
    public async Task Handle(QuestionsRemoveSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var link = await dbContext.QuestionSourceLinks
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.QuestionId == request.QuestionId &&
                    entity.Id == request.SourceLinkId,
                cancellationToken);

        if (link is null)
        {
            throw new ApiErrorException(
                $"Question source link '{request.SourceLinkId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.QuestionSourceLinks.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
