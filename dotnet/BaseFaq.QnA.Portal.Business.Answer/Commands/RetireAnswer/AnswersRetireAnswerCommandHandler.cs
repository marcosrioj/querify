using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Activity;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.RetireAnswer;

public sealed class AnswersRetireAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersRetireAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersRetireAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        entity.Status = AnswerStatus.Archived;
        entity.Visibility = VisibilityScope.Internal;
        entity.RetiredAtUtc = DateTime.UtcNow;

        var activityIdentity = dbContext.ResolveActivityIdentity(userId, null, null, userId);
        var activity = new ActivityEntity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.QuestionId,
            Question = entity.Question,
            AnswerId = entity.Id,
            Answer = entity,
            Kind = ActivityKind.AnswerRetired,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            UserPrint = activityIdentity.UserPrint,
            Ip = activityIdentity.Ip,
            UserAgent = activityIdentity.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        entity.Question.Activities.Add(activity);
        entity.Question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
