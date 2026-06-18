using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Domain.BusinessRules.Answers;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Answer.Commands.CreateAnswer;

public sealed class AnswersCreateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersCreateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersCreateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var actor = ActivityActorResolver.ResolvePortalActor(
            sessionService,
            httpContextAccessor,
            ActorKind.Moderator);
        var userId = actor.AuditUserId;
        var question = await dbContext.Questions
            .Include(entity => entity.Answers)
            .Include(entity => entity.Activities)
            .Include(entity => entity.Space)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionId,
                cancellationToken);

        if (question is null)
            throw new ApiErrorException(
                $"Question '{request.Request.QuestionId}' was not found.",
                (int)HttpStatusCode.NotFound);

        SpaceRules.EnsureAcceptsAnswers(question.Space);

        var entity = new Common.Domain.Entities.Answer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            Headline = request.Request.Headline,
            Kind = request.Request.Kind,
            Status = request.Request.Status,
            Visibility = request.Request.Visibility,
            AiConfidenceScore = 0,
            Score = 0,
            Sort = request.Request.Sort,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        question.Answers.Add(entity);
        question.LastActivityAtUtc = DateTime.UtcNow;
        dbContext.Answers.Add(entity);

        Apply(entity, request.Request, userId);
        await ApplyFollowUpQuestionsAsync(entity, request.Request.FollowUpQuestionIds, tenantId, userId, cancellationToken);
        var answerSnapshot = ActivityEntityMetadata.SnapshotAnswer(entity);
        ActivityAppender.AddAnswerActivity(
            entity,
            ActivityKind.AnswerCreated,
            actor,
            "Created",
            new Dictionary<string, object?>(StringComparer.Ordinal),
            answerSnapshot,
            ActivityEntityMetadata.AnswerContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(Common.Domain.Entities.Answer entity, AnswerCreateRequestDto request,
        string userId)
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
            .ToList();

        if (requestedIds.Count == 0)
            return;

        if (requestedIds.Contains(entity.QuestionId))
            throw new ApiErrorException(
                $"Answer '{entity.Id}' cannot link its own question as a follow-up question.",
                (int)HttpStatusCode.UnprocessableEntity);

        var followUpQuestions = await dbContext.Questions
            .Where(question => question.TenantId == tenantId && requestedIds.Contains(question.Id))
            .ToListAsync(cancellationToken);

        if (followUpQuestions.Count != requestedIds.Count)
        {
            var foundIds = followUpQuestions.Select(question => question.Id).ToHashSet();
            var missingId = requestedIds.First(id => !foundIds.Contains(id));
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
