using BaseFaq.Models.Ai.Contracts.Generation;
using MassTransit;

namespace BaseFaq.Faq.Portal.Api.Consumers;

public sealed class FaqGenerationReadyConsumer(ILogger<FaqGenerationReadyConsumer> logger)
    : IConsumer<FaqGenerationReadyV1>
{
    public Task Consume(ConsumeContext<FaqGenerationReadyV1> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Generation ready callback consumed. CorrelationId: {CorrelationId}, JobId: {JobId}, FaqId: {FaqId}, TenantId: {TenantId}, OccurredUtc: {OccurredUtc}",
            message.CorrelationId,
            message.JobId,
            message.FaqId,
            message.TenantId,
            message.OccurredUtc);

        return Task.CompletedTask;
    }
}