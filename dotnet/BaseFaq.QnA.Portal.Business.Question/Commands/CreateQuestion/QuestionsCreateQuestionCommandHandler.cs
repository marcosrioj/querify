using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.CreateQuestion;

public sealed class QuestionsCreateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsCreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsCreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var actor = ActivityActorResolver.ResolvePortalActor(
            sessionService,
            httpContextAccessor,
            ActorKind.Moderator);
        var userId = actor.AuditUserId;
        var space = await dbContext.Spaces
            .Include(entity => entity.Questions)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SpaceId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException($"Space '{request.Request.SpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (!space.AcceptsQuestions)
            throw new ApiErrorException(
                "This space is not accepting questions.",
                (int)HttpStatusCode.UnprocessableEntity);

        EnsureSupportedStatus(request.Request.Status);

        var entity = new Common.Domain.Entities.Question
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = request.Request.Title,
            Status = request.Request.Status,
            Visibility = request.Request.Visibility,
            OriginChannel = request.Request.OriginChannel,
            AiConfidenceScore = 0,
            FeedbackScore = 0,
            Sort = request.Request.Sort,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        space.Questions.Add(entity);
        dbContext.Questions.Add(entity);

        Apply(entity, request.Request, userId);
        var questionSnapshot = SnapshotQuestion(entity);
        ActivityAppender.AddQuestionActivity(
            entity,
            ActivityKind.QuestionCreated,
            actor,
            "Created",
            new Dictionary<string, object?>(StringComparer.Ordinal),
            questionSnapshot,
            QuestionContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(Common.Domain.Entities.Question entity, QuestionCreateRequestDto request, string userId)
    {
        EnsureSupportedStatus(request.Status);

        entity.Title = request.Title;
        entity.Summary = request.Summary;
        entity.ContextNote = request.ContextNote;
        entity.Sort = request.Sort;
        entity.Status = request.Status;

        EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }

    private static Dictionary<string, object?> SnapshotQuestion(Common.Domain.Entities.Question entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Id"] = entity.Id,
            ["TenantId"] = entity.TenantId,
            ["SpaceId"] = entity.SpaceId,
            ["Title"] = entity.Title,
            ["Summary"] = entity.Summary,
            ["ContextNote"] = entity.ContextNote,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString(),
            ["OriginChannel"] = entity.OriginChannel.ToString(),
            ["AiConfidenceScore"] = entity.AiConfidenceScore,
            ["FeedbackScore"] = entity.FeedbackScore,
            ["Sort"] = entity.Sort,
            ["AcceptedAnswerId"] = entity.AcceptedAnswerId
        };
    }

    private static Dictionary<string, object?> QuestionContext(Common.Domain.Entities.Question entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["QuestionId"] = entity.Id,
            ["SpaceId"] = entity.SpaceId,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString()
        };
    }

    private static void EnsureVisibilityAllowed(Common.Domain.Entities.Question entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not QuestionStatus.Active)
            throw new ApiErrorException(
                "Only active questions can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    private static void EnsureSupportedStatus(QuestionStatus status)
    {
        if (status is QuestionStatus.Draft or QuestionStatus.Active or QuestionStatus.Archived)
            return;

        throw new ApiErrorException(
            "Unsupported question status.",
            (int)HttpStatusCode.UnprocessableEntity);
    }
}
