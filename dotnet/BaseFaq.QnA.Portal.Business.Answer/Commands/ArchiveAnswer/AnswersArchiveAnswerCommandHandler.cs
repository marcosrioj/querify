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

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.ArchiveAnswer;

public sealed class AnswersArchiveAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersArchiveAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersArchiveAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var originalStatus = entity.Status;
        var originalVisibility = entity.Visibility;
        entity.Status = AnswerStatus.Archived;
        entity.Visibility = VisibilityScope.Internal;

        if (originalStatus != entity.Status)
        {
            var activityIdentity = ResolveActivityIdentity(userId);
            var activity = new Activity
            {
                TenantId = entity.TenantId,
                QuestionId = entity.QuestionId,
                Question = entity.Question,
                AnswerId = entity.Id,
                Answer = entity,
                Kind = ActivityKindStatusMap.ForAnswerStatus(entity.Status),
                ActorKind = ActorKind.Moderator,
                ActorLabel = userId,
                UserPrint = activityIdentity.UserPrint,
                Ip = activityIdentity.Ip,
                UserAgent = activityIdentity.UserAgent,
                MetadataJson = ActivityChangeMetadata.Create(
                    "Answer",
                    "StatusChanged",
                    entity.Id,
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["Status"] = originalStatus.ToString(),
                        ["Visibility"] = originalVisibility.ToString()
                    },
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                    {
                        ["Status"] = entity.Status.ToString(),
                        ["Visibility"] = entity.Visibility.ToString()
                    },
                    AnswerContext(entity),
                    maxLength: Activity.MaxMetadataLength),
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
}
