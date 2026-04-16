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

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RejectQuestion;

public sealed class QuestionsRejectQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsRejectQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsRejectQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Questions
            .Include(question => question.Activities)
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        entity.Status = QuestionStatus.Draft;
        entity.Visibility = VisibilityScope.Internal;
        AddThreadActivity(entity, ActivityKind.QuestionRejected, userId, request.Notes);
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

        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(activity);
    }
}
