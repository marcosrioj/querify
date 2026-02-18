using System.Diagnostics;
using BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;
using BaseFaq.AI.Business.Generation.Observability;
using BaseFaq.Models.Ai.Contracts.Generation;
using MassTransit;
using MediatR;

namespace BaseFaq.AI.Business.Generation.Consumers;

public sealed class FaqGenerationRequestedConsumer(
    IMediator mediator)
    : IConsumer<FaqGenerationRequestedV1>
{
    public async Task Consume(ConsumeContext<FaqGenerationRequestedV1> context)
    {
        using var consumeActivity =
            GenerationWorkerTracing.ActivitySource.StartActivity("generation.worker.consume",
                ActivityKind.Consumer);

        var handlerName = nameof(FaqGenerationRequestedConsumer);
        var messageId = context.MessageId?.ToString("N") ?? context.Message.CorrelationId.ToString("N");
        var message = context.Message;

        consumeActivity?.SetTag("messaging.system", "rabbitmq");
        consumeActivity?.SetTag("messaging.operation.name", "process");
        consumeActivity?.SetTag("messaging.message.id", messageId);
        consumeActivity?.SetTag("messaging.conversation_id", message.CorrelationId.ToString("D"));
        consumeActivity?.SetTag("basefaq.tenant_id", message.TenantId.ToString("D"));
        consumeActivity?.SetTag("basefaq.faq_id", message.FaqId.ToString("D"));

        await mediator.Send(
            new ProcessFaqGenerationRequestedCommand(message, handlerName, messageId),
            context.CancellationToken);
    }
}