using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
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
        var userId = sessionService.GetUserId().ToString();
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

        EnsureSupportedStatus(request.Request.Status);
        var beforeSnapshot = SnapshotQuestion(entity);
        var originalStatus = entity.Status;
        Apply(entity, request.Request, userId);

        if (!request.Request.AcceptedAnswerId.HasValue && entity.AcceptedAnswerId.HasValue)
        {
            entity.AcceptedAnswerId = null;
            entity.AcceptedAnswer = null;
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

            if (answer.QuestionId != entity.Id)
                throw new ApiErrorException(
                    $"Accepted answer '{acceptedAnswerId}' belongs to a different question.",
                    (int)HttpStatusCode.UnprocessableEntity);

            if (answer.Status is not AnswerStatus.Active)
                throw new ApiErrorException(
                    "Only active answers can be accepted.",
                    (int)HttpStatusCode.UnprocessableEntity);

            if (entity.Visibility is VisibilityScope.Public &&
                answer.Visibility is not VisibilityScope.Public)
                throw new ApiErrorException(
                    "Public questions cannot accept non-public answers.",
                    (int)HttpStatusCode.UnprocessableEntity);

            entity.AcceptedAnswerId = answer.Id;
            entity.AcceptedAnswer = answer;
            if (entity.Status is QuestionStatus.Draft)
                entity.Status = QuestionStatus.Active;
        }

        var afterSnapshot = SnapshotQuestion(entity);
        var statusChanged = entity.Status != originalStatus;
        var metadata = ActivityChangeMetadata.Create(
            "Question",
            statusChanged ? "StatusChanged" : "Updated",
            entity.Id,
            beforeSnapshot,
            afterSnapshot,
            QuestionContext(entity),
            maxLength: Activity.MaxMetadataLength);
        if (metadata is not null)
            AddActivity(
                entity,
                statusChanged
                    ? ActivityKindStatusMap.ForQuestionStatus(entity.Status)
                    : ActivityKind.QuestionUpdated,
                userId,
                metadata);

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private void AddActivity(
        Common.Persistence.QnADb.Entities.Question question,
        ActivityKind kind,
        string userId,
        string? metadataJson)
    {
        var activityIdentity = ResolveActivityIdentity(userId);
        var activity = new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = kind,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            UserPrint = activityIdentity.UserPrint,
            Ip = activityIdentity.Ip,
            UserAgent = activityIdentity.UserAgent,
            MetadataJson = metadataJson,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);
    }

    private ActivityUserIdentity ResolveActivityIdentity(string userId)
    {
        var httpContext = httpContextAccessor.HttpContext
                          ?? throw new ApiErrorException(
                              "HttpContext is missing from the current request.",
                              (int)HttpStatusCode.Unauthorized);
        return ActivityIdentityResolver.ResolveActivityIdentity(
            userId,
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext));
    }

    private static void Apply(Common.Persistence.QnADb.Entities.Question entity, QuestionUpdateRequestDto request,
        string userId)
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

    private static Dictionary<string, object?> SnapshotQuestion(Common.Persistence.QnADb.Entities.Question entity)
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

    private static Dictionary<string, object?> QuestionContext(Common.Persistence.QnADb.Entities.Question entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["QuestionId"] = entity.Id,
            ["SpaceId"] = entity.SpaceId,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString()
        };
    }

    private static void EnsureVisibilityAllowed(Common.Persistence.QnADb.Entities.Question entity,
        VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not QuestionStatus.Active)
            throw new ApiErrorException(
                "Only active questions can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);

        foreach (var sourceLink in entity.Sources)
            if (sourceLink.Role is SourceRole.Reference &&
                sourceLink.Source.Visibility is not VisibilityScope.Public)
                throw new ApiErrorException(
                    "Public references require a publicly visible source.",
                    (int)HttpStatusCode.UnprocessableEntity);

        if (entity.AcceptedAnswer is not null &&
            entity.AcceptedAnswer.Visibility is not VisibilityScope.Public)
            throw new ApiErrorException(
                "Public questions require a publicly visible accepted answer.",
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
