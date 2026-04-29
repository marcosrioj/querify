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

        var originalStatus = entity.Status;
        Apply(entity, request.Request, userId);
        if (entity.Status != originalStatus)
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
                entity.ActivatedAtUtc ??= DateTime.UtcNow;
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
