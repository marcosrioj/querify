using System.Text.Json;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Identity;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Projections;

public static class ActivitySignals
{
    public static int ComputeFeedbackScore(IEnumerable<Activity> activities)
    {
        return activities
            .Where(activity => activity.Kind == ActivityKind.FeedbackReceived)
            .Select(activity =>
            {
                var metadata = ParseFeedback(activity.MetadataJson);
                return new
                {
                    activity.OccurredAtUtc,
                    Metadata = metadata,
                    UserPrint = ActivityUserPrint.ResolveStored(activity.UserPrint, metadata?.UserPrint)
                };
            })
            .Where(item => item.Metadata is not null && !string.IsNullOrWhiteSpace(item.UserPrint))
            .GroupBy(item => item.UserPrint!, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(item => item.OccurredAtUtc).First().Metadata!)
            .Sum(metadata => metadata.Like ? 1 : -1);
    }

    public static int ComputeVoteScore(IEnumerable<Activity> activities, Guid answerId)
    {
        return activities
            .Where(activity => activity.Kind == ActivityKind.VoteReceived && activity.AnswerId == answerId)
            .Select(activity =>
            {
                var metadata = ParseVote(activity.MetadataJson);
                return new
                {
                    activity.OccurredAtUtc,
                    Metadata = metadata,
                    UserPrint = ActivityUserPrint.ResolveStored(activity.UserPrint, metadata?.UserPrint)
                };
            })
            .Where(item => item.Metadata is not null && !string.IsNullOrWhiteSpace(item.UserPrint))
            .GroupBy(item => item.UserPrint!, StringComparer.Ordinal)
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

    public static string CreateReportMetadata(
        string userPrint,
        string ip,
        string userAgent,
        string? reason)
    {
        return JsonSerializer.Serialize(new ReportSignalMetadata
        {
            UserPrint = userPrint,
            Ip = ip,
            UserAgent = userAgent,
            Reason = reason
        });
    }

    public static ReportSignalMetadata? ParseReport(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return null;

        try
        {
            return JsonSerializer.Deserialize<ReportSignalMetadata>(metadataJson);
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

    public sealed class ReportSignalMetadata
    {
        public required string UserPrint { get; init; }
        public string? Ip { get; init; }
        public string? UserAgent { get; init; }
        public string? Reason { get; init; }
    }
}
