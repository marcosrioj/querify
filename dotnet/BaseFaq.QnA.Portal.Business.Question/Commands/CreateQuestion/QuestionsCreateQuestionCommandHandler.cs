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
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
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

        var entity = new QuestionEntity
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = request.Request.Title,
            Status = request.Request.Status,
            Visibility = request.Request.Visibility,
            OriginChannel = request.Request.OriginChannel,
            AiConfidenceScore = request.Request.AiConfidenceScore,
            FeedbackScore = request.Request.FeedbackScore,
            Sort = request.Request.Sort,
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
        entity.Summary = request.Summary;
        entity.ContextNote = request.ContextNote;
        entity.AiConfidenceScore = request.AiConfidenceScore;
        entity.FeedbackScore = request.FeedbackScore;
        entity.Sort = request.Sort;
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
