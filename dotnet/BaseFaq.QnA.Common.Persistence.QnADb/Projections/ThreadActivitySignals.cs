using System.Text.Json;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Projections;

public static class ThreadActivitySignals
{
    public static int ComputeFeedbackScore(IEnumerable<ThreadActivity> activities)
    {
        return activities
            .Where(activity => activity.Kind == ActivityKind.FeedbackReceived)
            .Select(activity => new { activity.OccurredAtUtc, Metadata = ParseFeedback(activity.MetadataJson) })
            .Where(item => item.Metadata is not null)
            .GroupBy(item => item.Metadata!.UserPrint)
            .Select(group => group.OrderByDescending(item => item.OccurredAtUtc).First().Metadata!)
            .Sum(metadata => metadata.Like ? 1 : -1);
    }

    public static int ComputeVoteScore(IEnumerable<ThreadActivity> activities, Guid answerId)
    {
        return activities
            .Where(activity => activity.Kind == ActivityKind.VoteReceived && activity.AnswerId == answerId)
            .Select(activity => new { activity.OccurredAtUtc, Metadata = ParseVote(activity.MetadataJson) })
            .Where(item => item.Metadata is not null)
            .GroupBy(item => item.Metadata!.UserPrint)
            .Select(group => group.OrderByDescending(item => item.OccurredAtUtc).First().Metadata!)
            .Sum(metadata => metadata.VoteValue);
    }

    public static string CreateFeedbackMetadata(
        string userPrint,
        string ip,
        string userAgent,
        bool like,
        string? reason)
    {
        return JsonSerializer.Serialize(new FeedbackSignalMetadata
        {
            UserPrint = userPrint,
            Ip = ip,
            UserAgent = userAgent,
            Like = like,
            Reason = reason
        });
    }

    public static FeedbackSignalMetadata? ParseFeedback(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return null;

        try
        {
            return JsonSerializer.Deserialize<FeedbackSignalMetadata>(metadataJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string CreateVoteMetadata(
        string userPrint,
        string ip,
        string userAgent,
        int voteValue)
    {
        return JsonSerializer.Serialize(new VoteSignalMetadata
        {
            UserPrint = userPrint,
            Ip = ip,
            UserAgent = userAgent,
            VoteValue = voteValue
        });
    }

    public static VoteSignalMetadata? ParseVote(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return null;

        try
        {
            return JsonSerializer.Deserialize<VoteSignalMetadata>(metadataJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public sealed class FeedbackSignalMetadata
    {
        public required string UserPrint { get; init; }
        public string? Ip { get; init; }
        public string? UserAgent { get; init; }
        public required bool Like { get; init; }
        public string? Reason { get; init; }
    }

    public sealed class VoteSignalMetadata
    {
        public required string UserPrint { get; init; }
        public string? Ip { get; init; }
        public string? UserAgent { get; init; }
        public required int VoteValue { get; init; }
    }
}