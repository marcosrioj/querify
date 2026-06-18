using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Domain.BusinessRules.Answers;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Answer.Commands.UpdateAnswer;

public sealed class AnswersUpdateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersUpdateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersUpdateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var actor = ActivityActorResolver.ResolvePortalActor(
            sessionService,
            httpContextAccessor,
            ActorKind.Moderator);
        var userId = actor.AuditUserId;
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .Include(answer => answer.FollowUpQuestions)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var beforeSnapshot = ActivityEntityMetadata.SnapshotAnswer(entity);
        var originalStatus = entity.Status;
        Apply(entity, request.Request, userId);
        if (request.Request.FollowUpQuestionIds is not null)
            await ApplyFollowUpQuestionsAsync(
                entity,
                request.Request.FollowUpQuestionIds,
                tenantId,
                userId,
                cancellationToken);

        var afterSnapshot = ActivityEntityMetadata.SnapshotAnswer(entity);
        var statusChanged = entity.Status != originalStatus;
        ActivityAppender.AddAnswerActivity(
            entity,
            statusChanged
                ? ActivityKindStatusMap.ForAnswerStatus(entity.Status)
                : ActivityKind.AnswerUpdated,
            actor,
            statusChanged ? "StatusChanged" : "Updated",
            beforeSnapshot,
            afterSnapshot,
            ActivityEntityMetadata.AnswerContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(Common.Domain.Entities.Answer entity, AnswerUpdateRequestDto request, string userId)
    {
        entity.Headline = request.Headline;
        entity.Body = request.Body;
        entity.AuthorLabel = request.AuthorLabel;
        entity.ContextNote = request.ContextNote;
        entity.Sort = request.Sort;
        entity.Kind = request.Kind;

        AnswerRules.SetSupportedStatus(entity, request.Status);

        AnswerRules.EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }

    private async Task ApplyFollowUpQuestionsAsync(
        Common.Domain.Entities.Answer entity,
        IReadOnlyCollection<Guid> followUpQuestionIds,
        Guid tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        var requestedIds = followUpQuestionIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToHashSet();

        if (requestedIds.Contains(entity.QuestionId))
            throw new ApiErrorException(
                $"Answer '{entity.Id}' cannot link its own question as a follow-up question.",
                (int)HttpStatusCode.UnprocessableEntity);

        foreach (var existing in entity.FollowUpQuestions.Where(question => !requestedIds.Contains(question.Id)).ToList())
        {
            existing.ParentAnswerId = null;
            existing.ParentAnswer = null;
            existing.UpdatedBy = userId;
            entity.FollowUpQuestions.Remove(existing);
        }

        var existingIds = entity.FollowUpQuestions.Select(question => question.Id).ToHashSet();
        var missingIds = requestedIds.Except(existingIds).ToList();

        if (missingIds.Count == 0)
            return;

        var followUpQuestions = await dbContext.Questions
            .Where(question => question.TenantId == tenantId && missingIds.Contains(question.Id))
            .ToListAsync(cancellationToken);

        if (followUpQuestions.Count != missingIds.Count)
        {
            var foundIds = followUpQuestions.Select(question => question.Id).ToHashSet();
            var missingId = missingIds.First(id => !foundIds.Contains(id));
            throw new ApiErrorException(
                $"Follow-up question '{missingId}' was not found.",
                (int)HttpStatusCode.NotFound);
        }

        foreach (var question in followUpQuestions)
        {
            question.ParentAnswerId = entity.Id;
            question.ParentAnswer = entity;
            question.UpdatedBy = userId;
            entity.FollowUpQuestions.Add(question);
        }
    }
}
