using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.SubmitQuestion;

public sealed class QuestionsSubmitQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsSubmitQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsSubmitQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Questions
            .Include(question => question.Space)
            .Include(question => question.Activities)
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var targetStatus = RequiresReview(entity.Space.Kind) ? QuestionStatus.PendingReview : QuestionStatus.Open;
        entity.Status = targetStatus;
        AddActivity(entity, ActivityKind.QuestionSubmitted, userId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static bool RequiresReview(SpaceKind kind) =>
        kind is SpaceKind.ControlledPublication or SpaceKind.ModeratedCollaboration;

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
                          ?? throw new InvalidOperationException("HttpContext is missing from the current request.");
        return ActivityIdentityResolver.ResolveActivityIdentity(
            userId,
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext));
    }
}
