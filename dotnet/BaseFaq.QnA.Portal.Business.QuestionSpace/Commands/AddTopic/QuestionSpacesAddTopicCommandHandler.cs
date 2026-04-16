using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddTopic;

public sealed class QuestionSpacesAddTopicCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesAddTopicCommand, Guid>
{
    public async Task<Guid> Handle(QuestionSpacesAddTopicCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var space = await dbContext.QuestionSpaces
            .Include(entity => entity.Topics)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionSpaceId,
                cancellationToken);
        var topic = await dbContext.Topics
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.TopicId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException(
                $"Question space '{request.Request.QuestionSpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (topic is null)
            throw new ApiErrorException($"Topic '{request.Request.TopicId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (space.Topics.All(link => link.TopicId != topic.Id))
            space.Topics.Add(new QuestionSpaceTopic
            {
                TenantId = tenantId,
                QuestionSpaceId = space.Id,
                QuestionSpace = space,
                TopicId = topic.Id,
                Topic = topic,
                CreatedBy = userId,
                UpdatedBy = userId
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        return space.Topics.Single(link => link.TopicId == topic.Id).Id;
    }
}
