using System.Diagnostics;
using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ActivityKind = BaseFaq.Models.QnA.Enums.ActivityKind;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.EscalateQuestion;

public sealed class QuestionsEscalateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsEscalateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsEscalateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Questions
            .Include(question => question.Activities)
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        entity.Status = QuestionStatus.Escalated;
        AddActivity(entity, ActivityKind.QuestionEscalated, userId, request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private void AddActivity(Common.Persistence.QnADb.Entities.Question question, ActivityKind kind, string userId, string? notes = null)
    {
        var activityIdentity = ResolveActivityIdentity(userId);
        var activity = new Common.Persistence.QnADb.Entities.Activity
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
            Notes = notes,
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