using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;
using ActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Activity;

namespace BaseFaq.QnA.Public.Business.Question.Commands.CreateQuestion;

public sealed class QuestionsCreateQuestionCommandHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    ISessionService sessionService,
    IClaimService claimService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsCreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsCreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new ApiErrorException(
            "HttpContext is missing from the current request.",
            (int)HttpStatusCode.Unauthorized);
        var identity = ActivityIdentityResolver.ResolveActivityIdentity(
            sessionService,
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext),
            claimService.GetExternalUserId());
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var space = await dbContext.Spaces
            .Include(entity => entity.Questions)
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.Id == request.Request.SpaceId &&
                    (entity.Visibility == VisibilityScope.Public || entity.Visibility == VisibilityScope.PublicIndexed),
                cancellationToken);

        if (space is null)
            throw new ApiErrorException(
                $"Space '{request.Request.SpaceId}' was not found.",
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
            Key = request.Request.Key,
            Summary = request.Request.Summary,
            ContextNote = request.Request.ContextNote,
            ThreadSummary = request.Request.ThreadSummary,
            OriginChannel = request.Request.OriginChannel,
            Language = request.Request.Language,
            ProductScope = request.Request.ProductScope,
            JourneyScope = request.Request.JourneyScope,
            AudienceScope = request.Request.AudienceScope,
            ContextKey = request.Request.ContextKey,
            OriginUrl = request.Request.OriginUrl,
            OriginReference = request.Request.OriginReference,
            ConfidenceScore = request.Request.ConfidenceScore,
            RevisionNumber = 1,
            Status = RequiresReview(space.Kind) ? QuestionStatus.PendingReview : QuestionStatus.Open,
            Visibility = VisibilityScope.Internal,
            CreatedBy = "public",
            UpdatedBy = "public"
        };

        space.Questions.Add(entity);
        dbContext.Questions.Add(entity);

        var createdActivity = new ActivityEntity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.Id,
            Question = entity,
            Kind = ActivityKind.QuestionCreated,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = "public",
            UpdatedBy = "public"
        };
        entity.Activities.Add(createdActivity);
        entity.LastActivityAtUtc = createdActivity.OccurredAtUtc;
        dbContext.Activities.Add(createdActivity);

        var submittedActivity = new ActivityEntity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.Id,
            Question = entity,
            Kind = ActivityKind.QuestionSubmitted,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = "public",
            UpdatedBy = "public"
        };
        entity.Activities.Add(submittedActivity);
        entity.LastActivityAtUtc = submittedActivity.OccurredAtUtc;
        dbContext.Activities.Add(submittedActivity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static bool RequiresReview(SpaceKind kind) =>
        kind is SpaceKind.ControlledPublication or SpaceKind.ModeratedCollaboration;

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}
