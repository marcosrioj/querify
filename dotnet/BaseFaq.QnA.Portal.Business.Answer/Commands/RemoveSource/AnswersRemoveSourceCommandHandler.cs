using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.RemoveSource;

public sealed class AnswersRemoveSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersRemoveSourceCommand>
{
    public async Task Handle(AnswersRemoveSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var link = await dbContext.AnswerSourceLinks
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.AnswerId == request.AnswerId &&
                    entity.Id == request.SourceLinkId,
                cancellationToken);

        if (link is null)
            throw new ApiErrorException(
                $"Answer source link '{request.SourceLinkId}' was not found.",
                (int)HttpStatusCode.NotFound);

        dbContext.AnswerSourceLinks.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}