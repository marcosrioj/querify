using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;
using ThreadActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.ThreadActivity;

namespace BaseFaq.QnA.Portal.Business.Question.Commands;

public sealed class QuestionsApproveQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsApproveQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsApproveQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Questions
            .Include(question => question.Activity)
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException($"Question '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        entity.Status = QuestionStatus.Open;

        AddThreadActivity(entity, ActivityKind.QuestionApproved, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private void AddThreadActivity(QuestionEntity question, ActivityKind kind, string userId)
    {
        var activity = new ThreadActivityEntity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = kind,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        question.Activity.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(activity);
    }
}
