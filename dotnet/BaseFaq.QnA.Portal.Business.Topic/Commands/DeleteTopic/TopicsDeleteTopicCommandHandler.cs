using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands;

public sealed class TopicsDeleteTopicCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TopicsDeleteTopicCommand>
{
    public async Task Handle(TopicsDeleteTopicCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.Topics
            .SingleOrDefaultAsync(topic => topic.TenantId == tenantId && topic.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException($"Topic '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.Topics.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
