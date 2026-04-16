using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using BaseFaq.QnA.Public.Business.Feedback.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Feedback.Commands.CreateFeedback;

public sealed class FeedbacksCreateFeedbackCommandHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FeedbacksCreateFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(FeedbacksCreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var identity = FeedbackRequestContext.GetIdentity(httpContextAccessor);
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var question = await dbContext.Questions
            .Include(entity => entity.Activity)
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.Id == request.Request.QuestionId &&
                    (entity.Visibility == VisibilityScope.Public ||
                     entity.Visibility == VisibilityScope.PublicIndexed) &&
                    (entity.Status == QuestionStatus.Open || entity.Status == QuestionStatus.Answered ||
                     entity.Status == QuestionStatus.Validated),
                cancellationToken);

        if (question is null)
            throw new ApiErrorException(
                $"Question '{request.Request.QuestionId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var latest = question.Activity
            .Where(activity => activity.Kind == ActivityKind.FeedbackReceived)
            .Select(activity => new { activity, metadata = ThreadActivitySignals.ParseFeedback(activity.MetadataJson) })
            .Where(item => item.metadata?.UserPrint == identity.UserPrint)
            .OrderByDescending(item => item.activity.OccurredAtUtc)
            .FirstOrDefault();

        if (latest?.metadata is not null &&
            latest.metadata.Like == request.Request.Like &&
            latest.metadata.Reason == request.Request.Reason)
            return latest.activity.Id;

        var activity = new ThreadActivity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = ActivityKind.FeedbackReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            Notes = request.Request.Notes,
            MetadataJson = ThreadActivitySignals.CreateFeedbackMetadata(
                identity.UserPrint,
                identity.Ip,
                identity.UserAgent,
                request.Request.Like,
                request.Request.Reason),
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = identity.UserPrint,
            UpdatedBy = identity.UserPrint
        };

        question.Activity.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(activity);

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