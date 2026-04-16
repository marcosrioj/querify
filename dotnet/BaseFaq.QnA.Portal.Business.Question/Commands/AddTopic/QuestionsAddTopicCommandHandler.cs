using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.AddTopic;

public sealed class QuestionsAddTopicCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsAddTopicCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsAddTopicCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var question = await dbContext.Questions
            .Include(entity => entity.Topics)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionId,
                cancellationToken);
        var topic = await dbContext.Topics
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.TopicId,
                cancellationToken);

        if (question is null)
            throw new ApiErrorException($"Question '{request.Request.QuestionId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (topic is null)
            throw new ApiErrorException($"Topic '{request.Request.TopicId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (question.Topics.All(link => link.TopicId != topic.Id))
            question.Topics.Add(new QuestionTopic
            {
                TenantId = tenantId,
                QuestionId = question.Id,
                Question = question,
                TopicId = topic.Id,
                Topic = topic,
                CreatedBy = userId,
                UpdatedBy = userId
            });

        await dbContext.SaveChangesAsync(cancellationToken);
        return question.Topics.Single(link => link.TopicId == topic.Id).Id;
    }
}
