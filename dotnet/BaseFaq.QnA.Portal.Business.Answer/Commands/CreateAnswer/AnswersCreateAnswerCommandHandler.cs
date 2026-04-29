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

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.CreateAnswer;

public sealed class AnswersCreateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersCreateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersCreateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
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

        if (!question.Space.AcceptsAnswers)
            throw new ApiErrorException(
                "This space is not accepting answers.",
                (int)HttpStatusCode.UnprocessableEntity);

        var entity = new Common.Persistence.QnADb.Entities.Answer
        {
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
        AddActivity(question, entity, ActivityKindStatusMap.ForAnswerStatus(entity.Status), userId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private void AddActivity(
        Question question,
        Common.Persistence.QnADb.Entities.Answer answer,
        ActivityKind kind,
        string userId)
    {
        var activityIdentity = ResolveActivityIdentity(userId);
        var activity = new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer.Id,
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

    private static void Apply(Common.Persistence.QnADb.Entities.Answer entity, AnswerCreateRequestDto request,
        string userId)
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
                entity.ActivatedAtUtc = DateTime.UtcNow;
                break;
            case AnswerStatus.Archived:
                entity.Status = AnswerStatus.Archived;
                entity.RetiredAtUtc ??= DateTime.UtcNow;
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

    private static void EnsureVisibilityAllowed(Common.Persistence.QnADb.Entities.Answer entity,
        VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not AnswerStatus.Active)
            throw new ApiErrorException(
                "Only active answers can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);
    }
}
