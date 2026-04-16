using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.DeleteKnowledgeSource;

public sealed class KnowledgeSourcesDeleteKnowledgeSourceCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<KnowledgeSourcesDeleteKnowledgeSourceCommand>
{
    public async Task Handle(KnowledgeSourcesDeleteKnowledgeSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.KnowledgeSources
            .SingleOrDefaultAsync(source => source.TenantId == tenantId && source.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException(
                $"Knowledge source '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.KnowledgeSources.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
