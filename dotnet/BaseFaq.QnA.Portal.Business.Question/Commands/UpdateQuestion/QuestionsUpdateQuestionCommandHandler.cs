using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Activities;
using BaseFaq.QnA.Common.Domain.BusinessRules.Questions;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;

public sealed class QuestionsUpdateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsUpdateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsUpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var actor = ActivityActorResolver.ResolvePortalActor(
            sessionService,
            httpContextAccessor,
            ActorKind.Moderator);
        var userId = actor.AuditUserId;
        var entity = await dbContext.Questions
            .Include(question => question.Answers)
            .Include(question => question.AcceptedAnswer)
            .Include(question => question.Activities)
            .Include(question => question.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        QuestionRules.EnsureSupportedStatus(request.Request.Status);
        var beforeSnapshot = ActivityEntityMetadata.SnapshotQuestion(entity);
        var originalStatus = entity.Status;
        Apply(entity, request.Request, userId);

        if (!request.Request.AcceptedAnswerId.HasValue && entity.AcceptedAnswerId.HasValue)
        {
            QuestionRules.ClearAcceptedAnswer(entity);
        }

        if (request.Request.AcceptedAnswerId is Guid acceptedAnswerId && acceptedAnswerId != entity.AcceptedAnswerId)
        {
            var answer = entity.Answers.SingleOrDefault(candidate => candidate.Id == acceptedAnswerId)
                         ?? await dbContext.Answers
                             .Include(candidate => candidate.Question)
                             .SingleOrDefaultAsync(
                                 candidate => candidate.TenantId == tenantId && candidate.Id == acceptedAnswerId,
                                 cancellationToken);

            if (answer is null)
                throw new ApiErrorException($"Answer '{acceptedAnswerId}' was not found.",
                    (int)HttpStatusCode.NotFound);

            QuestionRules.ApplyAcceptedAnswer(entity, answer);
        }

        var afterSnapshot = ActivityEntityMetadata.SnapshotQuestion(entity);
        var statusChanged = entity.Status != originalStatus;
        ActivityAppender.AddQuestionActivity(
            entity,
            statusChanged
                ? ActivityKindStatusMap.ForQuestionStatus(entity.Status)
                : ActivityKind.QuestionUpdated,
            actor,
            statusChanged ? "StatusChanged" : "Updated",
            beforeSnapshot,
            afterSnapshot,
            ActivityEntityMetadata.QuestionContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(Common.Domain.Entities.Question entity, QuestionUpdateRequestDto request,
        string userId)
    {
        QuestionRules.EnsureSupportedStatus(request.Status);

        entity.Title = request.Title;
        entity.Summary = request.Summary;
        entity.ContextNote = request.ContextNote;
        entity.Sort = request.Sort;
        entity.Status = request.Status;

        QuestionRules.EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }
}
