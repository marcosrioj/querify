namespace BaseFaq.Models.Ai.Contracts.Generation;

public static class GenerationEventNames
{
    public const string ReadyExchange = "faq.generation.ready.v1";
    public const string FailedExchange = "faq.generation.failed.v1";
    public const string ReadyCallbackQueue = "faq.portal.generation.ready.v1";
    public const string FailedCallbackQueue = "faq.portal.generation.failed.v1";
    public const string ExchangeType = "fanout";
}