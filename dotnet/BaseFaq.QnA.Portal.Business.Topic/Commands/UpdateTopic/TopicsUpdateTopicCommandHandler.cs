using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands;

public sealed class TopicsUpdateTopicCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TopicsUpdateTopicCommand, Guid>
{
    public async Task<Guid> Handle(TopicsUpdateTopicCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Topics
            .SingleOrDefaultAsync(topic => topic.TenantId == tenantId && topic.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException($"Topic '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        entity.Name = request.Request.Name;
        entity.Category = request.Request.Category;
        entity.Description = request.Request.Description;
        entity.UpdatedBy = userId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
