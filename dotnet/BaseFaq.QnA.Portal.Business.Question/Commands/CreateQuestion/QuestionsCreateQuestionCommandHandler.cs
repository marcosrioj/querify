using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;
using ActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Activity;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.CreateQuestion;

public sealed class QuestionsCreateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsCreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsCreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var space = await dbContext.Spaces
            .Include(entity => entity.Questions)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SpaceId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException($"Space '{request.Request.SpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var entity = new QuestionEntity
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = request.Request.Title,
            Key = request.Request.Key,
            Kind = request.Request.Kind,
            Status = request.Request.Status,
            Visibility = request.Request.Visibility,
            OriginChannel = request.Request.OriginChannel,
            ConfidenceScore = request.Request.ConfidenceScore,
            RevisionNumber = 0,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        space.Questions.Add(entity);
        dbContext.Questions.Add(entity);

        Apply(entity, request.Request, userId);
        AddActivity(entity, ActivityKind.QuestionCreated, userId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private void AddActivity(QuestionEntity question, ActivityKind kind, string userId)
    {
        var activityIdentity = ResolveActivityIdentity(userId);
        var activity = new ActivityEntity
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
                          ?? throw new InvalidOperationException("HttpContext is missing from the current request.");
        return ActivityIdentityResolver.ResolveActivityIdentity(
            userId,
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext));
    }

    private static void Apply(QuestionEntity entity, QuestionCreateRequestDto request, string userId)
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
    }
}
