using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands.CreateTopic;

public sealed class TopicsCreateTopicCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TopicsCreateTopicCommand, Guid>
{
    public async Task<Guid> Handle(TopicsCreateTopicCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = new Common.Persistence.QnADb.Entities.Topic
        {
            TenantId = tenantId,
            Name = request.Request.Name,
            Category = request.Request.Category,
            Description = request.Request.Description,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        dbContext.Topics.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
