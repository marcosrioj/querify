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

        EnsureSupportedStatus(request.Request.Status);

        var entity = new Common.Persistence.QnADb.Entities.Question
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
        AddActivity(entity, ActivityKind.QuestionCreated, userId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private void AddActivity(Common.Persistence.QnADb.Entities.Question question, ActivityKind kind, string userId)
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

    private static void Apply(Common.Persistence.QnADb.Entities.Question entity, QuestionCreateRequestDto request, string userId)
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

    private static void EnsureVisibilityAllowed(Common.Persistence.QnADb.Entities.Question entity, VisibilityScope visibility)
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
