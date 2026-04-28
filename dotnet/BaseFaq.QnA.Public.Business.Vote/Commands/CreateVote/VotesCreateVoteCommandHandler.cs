using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Vote.Commands.CreateVote;

public sealed class VotesCreateVoteCommandHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    ISessionService sessionService,
    IClaimService claimService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<VotesCreateVoteCommand, Guid>
{
    public async Task<Guid> Handle(VotesCreateVoteCommand request, CancellationToken cancellationToken)
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
        var answer = await dbContext.Answers
            .Include(entity => entity.Question)
            .ThenInclude(question => question.Activities)
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.Id == request.Request.AnswerId &&
                    entity.QuestionId == request.Request.QuestionId &&
                    (entity.Visibility == VisibilityScope.Public ||
                     entity.Visibility == VisibilityScope.PublicIndexed) &&
                    (entity.Status == AnswerStatus.Published || entity.Status == AnswerStatus.Validated),
                cancellationToken);

        if (answer is null ||
            answer.Question.TenantId != tenantId ||
            answer.Question.Visibility is not (VisibilityScope.Public or VisibilityScope.PublicIndexed))
            throw new ApiErrorException(
                $"Answer '{request.Request.AnswerId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var latest = answer.Question.Activities
            .Where(activity =>
                activity.Kind == ActivityKind.VoteReceived && activity.AnswerId == request.Request.AnswerId)
            .Select(activity =>
            {
                var metadata = ActivitySignals.ParseVote(activity.MetadataJson);
                return new
                {
                    Activity = activity,
                    Metadata = metadata,
                    UserPrint = ActivityIdentityResolver.ResolveStored(activity.UserPrint, metadata?.UserPrint)
                };
            })
            .Where(item => item.Metadata is not null && item.UserPrint == identity.UserPrint)
            .OrderByDescending(item => item.Activity.OccurredAtUtc)
            .FirstOrDefault();

        var requestedValue = request.Request.IsUpvote ? 1 : -1;
        var effectiveValue = latest?.Metadata?.VoteValue;
        var storedValue = effectiveValue == requestedValue ? 0 : requestedValue;

        var activity = new Activity
        {
            TenantId = answer.TenantId,
            QuestionId = answer.QuestionId,
            Question = answer.Question,
            AnswerId = answer.Id,
            Answer = answer,
            Kind = ActivityKind.VoteReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            Notes = request.Request.Notes,
            MetadataJson = ActivitySignals.CreateVoteMetadata(
                identity.UserPrint,
                identity.Ip,
                identity.UserAgent,
                storedValue),
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = identity.UserPrint,
            UpdatedBy = identity.UserPrint
        };

        answer.Question.Activities.Add(activity);
        answer.Question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return storedValue == 0 ? Guid.Empty : activity.Id;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}