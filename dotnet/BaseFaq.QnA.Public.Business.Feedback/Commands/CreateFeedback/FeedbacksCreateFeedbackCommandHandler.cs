using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Feedback.Commands.CreateFeedback;

public sealed class FeedbacksCreateFeedbackCommandHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    ISessionService sessionService,
    IClaimService claimService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FeedbacksCreateFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(FeedbacksCreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var actor = ActivityActorResolver.ResolvePublicActor(
            sessionService,
            claimService,
            httpContextAccessor,
            ActorKind.Customer);
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var question = await dbContext.Questions
            .Include(entity => entity.Activities)
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.Id == request.Request.QuestionId &&
                    entity.Visibility == VisibilityScope.Public &&
                    entity.Status == QuestionStatus.Active,
                cancellationToken);

        if (question is null)
            throw new ApiErrorException(
                $"Question '{request.Request.QuestionId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var latest = question.Activities
            .Where(activity => activity.Kind == ActivityKind.FeedbackReceived)
            .Select(activity =>
            {
                var metadata = ActivitySignals.ParseFeedback(activity.MetadataJson);
                return new
                {
                    Activity = activity,
                    Metadata = metadata,
                    UserPrint = ActivityIdentityResolver.ResolveStored(activity.UserPrint, metadata?.UserPrint)
                };
            })
            .Where(item => item.Metadata is not null && item.UserPrint == actor.UserPrint)
            .OrderByDescending(item => item.Activity.OccurredAtUtc)
            .FirstOrDefault();

        if (latest?.Metadata is not null &&
            latest.Metadata.Like == request.Request.Like &&
            latest.Metadata.Reason == request.Request.Reason)
            return latest.Activity.Id;

        var activity = ActivityAppender.AddFeedbackActivity(
            question,
            actor,
            request.Request.Like,
            request.Request.Reason,
            request.Request.Notes);
        question.FeedbackScore = ActivitySignals.ComputeFeedbackScore(
            question.Activities.Select(ActivityEntityMetadata.ToSignalEntry));

        await dbContext.SaveChangesAsync(cancellationToken);
        return activity.Id;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

}
