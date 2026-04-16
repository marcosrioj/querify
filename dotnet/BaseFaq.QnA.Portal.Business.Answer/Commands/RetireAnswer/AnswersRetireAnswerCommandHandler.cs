using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThreadActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.ThreadActivity;

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
            .ThenInclude(question => question.Activity)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        entity.Status = AnswerStatus.Archived;
        entity.Visibility = VisibilityScope.Internal;
        entity.RetiredAtUtc = DateTime.UtcNow;

        var activity = new ThreadActivityEntity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.QuestionId,
            Question = entity.Question,
            AnswerId = entity.Id,
            Answer = entity,
            Kind = ActivityKind.AnswerRejected,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        entity.Question.Activity.Add(activity);
        entity.Question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
