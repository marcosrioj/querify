namespace BaseFaq.Models.Ai.Contracts.Matching;

public static class MatchingEventNames
{
    public const string RequestedExchange = "faq.matching.requested.v1";
    public const string CompletedExchange = "faq.matching.completed.v1";
    public const string FailedExchange = "faq.matching.failed.v1";
    public const string ExchangeType = "fanout";
}