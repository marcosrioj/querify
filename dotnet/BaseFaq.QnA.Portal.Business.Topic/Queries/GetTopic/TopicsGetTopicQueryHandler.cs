using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Topic.Queries.GetTopic;

public sealed class TopicsGetTopicQueryHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TopicsGetTopicQuery, TopicDto>
{
    public async Task<TopicDto> Handle(TopicsGetTopicQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.Topics.AsNoTracking()
            .SingleOrDefaultAsync(topic => topic.TenantId == tenantId && topic.Id == request.Id, cancellationToken);

        return entity is null
            ? throw new ApiErrorException($"Topic '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound)
            : new TopicDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Name = entity.Name,
                Category = entity.Category,
                Description = entity.Description
            };
    }
}
