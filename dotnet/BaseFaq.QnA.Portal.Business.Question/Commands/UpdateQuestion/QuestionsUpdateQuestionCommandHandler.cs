using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;
using ActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Activity;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;

public sealed class QuestionsUpdateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsUpdateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsUpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
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

        Apply(entity, request.Request, userId);

        if (!request.Request.DuplicateOfQuestionId.HasValue && entity.DuplicateOfQuestionId.HasValue)
        {
            entity.DuplicateOfQuestionId = null;
            entity.DuplicateOfQuestion = null;
        }

        if (request.Request.DuplicateOfQuestionId is Guid duplicateId && duplicateId != entity.DuplicateOfQuestionId)
        {
            var canonical = await dbContext.Questions
                .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == duplicateId,
                    cancellationToken);

            if (canonical is null)
                throw new ApiErrorException($"Question '{duplicateId}' was not found.", (int)HttpStatusCode.NotFound);

            entity.DuplicateOfQuestionId = canonical.Id;
            entity.DuplicateOfQuestion = canonical;
            entity.Status = QuestionStatus.Duplicate;

            if (canonical.DuplicateQuestions.All(existing => existing.Id != entity.Id))
                canonical.DuplicateQuestions.Add(entity);

            AddActivity(
                entity,
                ActivityKind.QuestionMarkedDuplicate,
                userId);
        }

        if (!request.Request.AcceptedAnswerId.HasValue && entity.AcceptedAnswerId.HasValue)
        {
            if (entity.AcceptedAnswer is not null)
                entity.AcceptedAnswer.AcceptedAtUtc = null;

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
                throw new InvalidOperationException(
                    $"Accepted answer '{acceptedAnswerId}' belongs to a different question.");

            if (answer.Status is not AnswerStatus.Published and not AnswerStatus.Validated)
                throw new InvalidOperationException("Only published or validated answers can be accepted.");

            if (entity.Visibility is VisibilityScope.Public or VisibilityScope.PublicIndexed &&
                answer.Visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed)
                throw new InvalidOperationException("Public questions cannot accept internal-only answers.");

            var acceptedAtUtc = DateTime.UtcNow;
            entity.AcceptedAnswerId = answer.Id;
            entity.AcceptedAnswer = answer;
            entity.AnsweredAtUtc ??= acceptedAtUtc;
            entity.ResolvedAtUtc = acceptedAtUtc;
            entity.Status = entity.Status == QuestionStatus.Validated
                ? QuestionStatus.Validated
                : QuestionStatus.Answered;
            answer.AcceptedAtUtc = acceptedAtUtc;

            AddActivity(entity, ActivityKind.AnswerAccepted, userId, answer);
        }

        AddActivity(entity, ActivityKind.QuestionUpdated, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private void AddActivity(
        QuestionEntity question,
        ActivityKind kind,
        string userId,
        Answer? answer = null)
    {
        var activityIdentity = ResolveActivityIdentity(userId);
        var activity = new ActivityEntity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer?.Id,
            Answer = answer,
            Kind = kind,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            UserPrint = activityIdentity.UserPrint,
            Ip = activityIdentity.Ip,
            UserAgent = activityIdentity.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        if (kind == ActivityKind.QuestionMarkedDuplicate &&
            question.DuplicateOfQuestionId is Guid duplicateOfQuestionId)
            activity.MetadataJson = $"{{\"duplicateOfQuestionId\":\"{duplicateOfQuestionId}\"}}";

        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);
    }

    private ActivityUserIdentity ResolveActivityIdentity(string userId)
    {
        var httpContext = httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("HttpContext is missing from the current request.");
        return ActivityIdentityResolver.ResolveActivityIdentity(
            userId,
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext));
    }

    private static void Apply(QuestionEntity entity, QuestionUpdateRequestDto request, string userId)
    {
        entity.Title = request.Title;
        entity.Key = request.Key;
        entity.Summary = request.Summary;
        entity.ContextNote = request.ContextNote;
        entity.ThreadSummary = request.ThreadSummary;
        entity.Language = request.Language;
        entity.ProductScope = request.ProductScope;
        entity.JourneyScope = request.JourneyScope;
        entity.AudienceScope = request.AudienceScope;
        entity.ContextKey = request.ContextKey;
        entity.OriginUrl = request.OriginUrl;
        entity.OriginReference = request.OriginReference;
        entity.ConfidenceScore = request.ConfidenceScore;
        entity.RevisionNumber++;
        entity.Status = request.Status;

        if (request.Status == QuestionStatus.Validated) entity.ValidatedAtUtc = DateTime.UtcNow;

        EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }

    private static void EnsureVisibilityAllowed(QuestionEntity entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed) return;

        if (entity.Status is not QuestionStatus.Open and not QuestionStatus.Answered and not QuestionStatus.Validated)
            throw new InvalidOperationException(
                "Only open, answered, or validated questions can be exposed publicly.");

        foreach (var sourceLink in entity.Sources)
            if (sourceLink.Role is SourceRole.Citation or SourceRole.CanonicalReference &&
                (sourceLink.Source.Visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed ||
                 !sourceLink.Source.AllowsPublicCitation))
                throw new InvalidOperationException(
                    "Public citations require a publicly visible source that explicitly allows citation.");

        if (entity.AcceptedAnswer is not null &&
            entity.AcceptedAnswer.Visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed)
            throw new InvalidOperationException("Public questions require a publicly visible accepted answer.");
    }
}
