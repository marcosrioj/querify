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

namespace BaseFaq.QnA.Portal.Business.Question.Commands.EscalateQuestion;

public sealed class QuestionsEscalateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsEscalateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsEscalateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Questions
            .Include(question => question.Activity)
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        entity.Status = QuestionStatus.Escalated;
        AddThreadActivity(entity, ActivityKind.QuestionEscalated, userId, request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private void AddThreadActivity(QuestionEntity question, ActivityKind kind, string userId, string? notes = null)
    {
        var activity = new ThreadActivityEntity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = kind,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            Notes = notes,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        question.Activity.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(activity);
    }
}