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

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.ValidateAnswer;

public sealed class AnswersValidateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersValidateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersValidateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        entity.Status = AnswerStatus.Validated;
        entity.ValidatedAtUtc = DateTime.UtcNow;

        var activityIdentity = ResolveActivityIdentity(userId);
        var activity = new Activity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.QuestionId,
            Question = entity.Question,
            AnswerId = entity.Id,
            Answer = entity,
            Kind = ActivityKind.AnswerValidated,
            ActorKind = ActorKind.Moderator,
            ActorLabel = userId,
            UserPrint = activityIdentity.UserPrint,
            Ip = activityIdentity.Ip,
            UserAgent = activityIdentity.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        entity.Question.Activities.Add(activity);
        entity.Question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
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
