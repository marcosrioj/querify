using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.UpdateAnswer;

public sealed class AnswersUpdateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersUpdateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersUpdateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var beforeSnapshot = SnapshotAnswer(entity);
        var originalStatus = entity.Status;
        Apply(entity, request.Request, userId);

        var afterSnapshot = SnapshotAnswer(entity);
        var statusChanged = entity.Status != originalStatus;
        var metadata = ActivityChangeMetadata.Create(
            "Answer",
            statusChanged ? "StatusChanged" : "Updated",
            entity.Id,
            beforeSnapshot,
            afterSnapshot,
            AnswerContext(entity),
            maxLength: Activity.MaxMetadataLength);
        if (metadata is not null)
        {
            var activityIdentity = ResolveActivityIdentity(userId);
            var activity = new Activity
            {
                TenantId = entity.TenantId,
                QuestionId = entity.QuestionId,
                Question = entity.Question,
                AnswerId = entity.Id,
                Answer = entity,
                Kind = statusChanged
                    ? ActivityKindStatusMap.ForAnswerStatus(entity.Status)
                    : ActivityKind.AnswerUpdated,
                ActorKind = ActorKind.Moderator,
                ActorLabel = userId,
                UserPrint = activityIdentity.UserPrint,
                Ip = activityIdentity.Ip,
                UserAgent = activityIdentity.UserAgent,
                MetadataJson = metadata,
                OccurredAtUtc = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };
            entity.Question.Activities.Add(activity);
            entity.Question.LastActivityAtUtc = activity.OccurredAtUtc;
            dbContext.Activities.Add(activity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
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

    private static void Apply(Common.Persistence.QnADb.Entities.Answer entity, AnswerUpdateRequestDto request, string userId)
    {
        entity.Headline = request.Headline;
        entity.Body = request.Body;
        entity.AuthorLabel = request.AuthorLabel;
        entity.ContextNote = request.ContextNote;
        entity.Sort = request.Sort;
        entity.Kind = request.Kind;

        switch (request.Status)
        {
            case AnswerStatus.Active:
                entity.Status = AnswerStatus.Active;
                break;
            case AnswerStatus.Archived:
                entity.Status = AnswerStatus.Archived;
                break;
            case AnswerStatus.Draft:
                entity.Status = request.Status;
                break;
            default:
                throw new ApiErrorException(
                    "Unsupported answer status.",
                    (int)HttpStatusCode.UnprocessableEntity);
        }

        EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }

    private static Dictionary<string, object?> SnapshotAnswer(Common.Persistence.QnADb.Entities.Answer entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Id"] = entity.Id,
            ["TenantId"] = entity.TenantId,
            ["QuestionId"] = entity.QuestionId,
            ["Headline"] = entity.Headline,
            ["Body"] = entity.Body,
            ["AuthorLabel"] = entity.AuthorLabel,
            ["ContextNote"] = entity.ContextNote,
            ["Kind"] = entity.Kind.ToString(),
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString(),
            ["AiConfidenceScore"] = entity.AiConfidenceScore,
            ["Score"] = entity.Score,
            ["Sort"] = entity.Sort
        };
    }

    private static Dictionary<string, object?> AnswerContext(Common.Persistence.QnADb.Entities.Answer entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["QuestionId"] = entity.QuestionId,
            ["AnswerId"] = entity.Id,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString()
        };
    }

    private static void EnsureVisibilityAllowed(Common.Persistence.QnADb.Entities.Answer entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not AnswerStatus.Active)
            throw new ApiErrorException(
                "Only active answers can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);

        foreach (var sourceLink in entity.Sources)
            if (sourceLink.Role is SourceRole.Reference &&
                sourceLink.Source.Visibility is not VisibilityScope.Public)
                throw new ApiErrorException(
                    "Public references require a publicly visible source.",
                    (int)HttpStatusCode.UnprocessableEntity);
    }
}
