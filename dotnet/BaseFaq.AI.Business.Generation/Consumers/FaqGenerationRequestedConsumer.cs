using BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;
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
        await mediator.Send(
            new ProcessFaqGenerationRequestedCommand(context.Message),
            context.CancellationToken);
    }
}