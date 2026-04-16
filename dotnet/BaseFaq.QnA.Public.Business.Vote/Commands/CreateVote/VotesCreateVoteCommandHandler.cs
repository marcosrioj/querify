using System.Net;
using System.Text.Json;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Public.Business.Vote.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Vote.Commands;

public sealed class VotesCreateVoteCommandHandler(
    QnADbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<VotesCreateVoteCommand, Guid>
{
    public async Task<Guid> Handle(VotesCreateVoteCommand request, CancellationToken cancellationToken)
    {
        var identity = VoteRequestContext.GetIdentity(httpContextAccessor);
        var answer = await dbContext.Answers
            .Include(entity => entity.Question)
            .ThenInclude(question => question.Activity)
            .SingleOrDefaultAsync(
                entity =>
                    entity.Id == request.Request.AnswerId &&
                    entity.QuestionId == request.Request.QuestionId &&
                    (entity.Visibility == VisibilityScope.Public || entity.Visibility == VisibilityScope.PublicIndexed) &&
                    (entity.Status == AnswerStatus.Published || entity.Status == AnswerStatus.Validated),
                cancellationToken);

        if (answer is null ||
            answer.Question.Visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed)
        {
            throw new ApiErrorException(
                $"Answer '{request.Request.AnswerId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var latest = answer.Question.Activity
            .Where(activity => activity.Kind == ActivityKind.VoteReceived && activity.AnswerId == request.Request.AnswerId)
            .Select(activity => new { activity, metadata = ParseVote(activity.MetadataJson) })
            .Where(item => item.metadata?.UserPrint == identity.UserPrint)
            .OrderByDescending(item => item.activity.OccurredAtUtc)
            .FirstOrDefault();

        var requestedValue = request.Request.IsUpvote ? 1 : -1;
        var effectiveValue = latest?.metadata?.VoteValue;
        var storedValue = effectiveValue == requestedValue ? 0 : requestedValue;

        var activity = new Common.Persistence.QnADb.Entities.ThreadActivity
        {
            TenantId = answer.TenantId,
            QuestionId = answer.QuestionId,
            Question = answer.Question,
            AnswerId = answer.Id,
            Answer = answer,
            Kind = ActivityKind.VoteReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            Notes = request.Request.Notes,
            MetadataJson = CreateVoteMetadata(identity, storedValue),
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = identity.UserPrint,
            UpdatedBy = identity.UserPrint
        };

        answer.Question.Activity.Add(activity);
        answer.Question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return storedValue == 0 ? Guid.Empty : activity.Id;
    }

    private static string CreateVoteMetadata(VoteRequestIdentity identity, int voteValue)
    {
        return JsonSerializer.Serialize(new VoteMetadata
        {
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            VoteValue = voteValue
        });
    }

    private static VoteMetadata? ParseVote(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<VoteMetadata>(metadataJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class VoteMetadata
    {
        public required string UserPrint { get; init; }
        public required string Ip { get; init; }
        public required string UserAgent { get; init; }
        public required int VoteValue { get; init; }
    }
}
