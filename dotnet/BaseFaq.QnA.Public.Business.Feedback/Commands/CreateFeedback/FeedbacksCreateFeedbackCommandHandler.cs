using System.Net;
using System.Text.Json;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Public.Business.Feedback.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Feedback.Commands;

public sealed class FeedbacksCreateFeedbackCommandHandler(
    QnADbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<FeedbacksCreateFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(FeedbacksCreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var identity = FeedbackRequestContext.GetIdentity(httpContextAccessor);
        var question = await dbContext.Questions
            .Include(entity => entity.Activity)
            .SingleOrDefaultAsync(
                entity =>
                    entity.Id == request.Request.QuestionId &&
                    (entity.Visibility == VisibilityScope.Public || entity.Visibility == VisibilityScope.PublicIndexed) &&
                    (entity.Status == QuestionStatus.Open ||
                     entity.Status == QuestionStatus.Answered ||
                     entity.Status == QuestionStatus.Validated),
                cancellationToken);

        if (question is null)
        {
            throw new ApiErrorException(
                $"Question '{request.Request.QuestionId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var latest = question.Activity
            .Where(activity => activity.Kind == ActivityKind.FeedbackReceived)
            .Select(activity => new { activity, metadata = ParseFeedback(activity.MetadataJson) })
            .Where(item => item.metadata?.UserPrint == identity.UserPrint)
            .OrderByDescending(item => item.activity.OccurredAtUtc)
            .FirstOrDefault();

        if (latest?.metadata is not null &&
            latest.metadata.Like == request.Request.Like &&
            latest.metadata.Reason == request.Request.Reason)
        {
            return latest.activity.Id;
        }

        var activity = new Common.Persistence.QnADb.Entities.ThreadActivity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            Kind = ActivityKind.FeedbackReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            Notes = request.Request.Notes,
            MetadataJson = CreateFeedbackMetadata(identity, request.Request.Like, request.Request.Reason),
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

    private static string CreateFeedbackMetadata(FeedbackRequestIdentity identity, bool like, string? reason)
    {
        return JsonSerializer.Serialize(new FeedbackMetadata
        {
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            Like = like,
            Reason = reason
        });
    }

    private static FeedbackMetadata? ParseFeedback(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<FeedbackMetadata>(metadataJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class FeedbackMetadata
    {
        public required string UserPrint { get; init; }
        public required string Ip { get; init; }
        public required string UserAgent { get; init; }
        public required bool Like { get; init; }
        public string? Reason { get; init; }
    }
}
