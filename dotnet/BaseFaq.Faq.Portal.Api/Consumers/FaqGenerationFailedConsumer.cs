using BaseFaq.Models.Ai.Contracts.Generation;
using MassTransit;

namespace BaseFaq.Faq.Portal.Api.Consumers;

public sealed class FaqGenerationFailedConsumer(ILogger<FaqGenerationFailedConsumer> logger)
    : IConsumer<FaqGenerationFailedV1>
{
    public Task Consume(ConsumeContext<FaqGenerationFailedV1> context)
    {
        var message = context.Message;

        logger.LogWarning(
            "Generation failed callback consumed. CorrelationId: {CorrelationId}, JobId: {JobId}, FaqId: {FaqId}, TenantId: {TenantId}, ErrorCode: {ErrorCode}, OccurredUtc: {OccurredUtc}",
            message.CorrelationId,
            message.JobId,
            message.FaqId,
            message.TenantId,
            message.ErrorCode,
            message.OccurredUtc);

        return Task.CompletedTask;
    }
}