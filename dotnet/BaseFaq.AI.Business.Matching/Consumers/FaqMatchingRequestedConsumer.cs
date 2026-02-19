using BaseFaq.AI.Business.Matching.Commands.ProcessFaqMatchingRequested;
using BaseFaq.Models.Ai.Contracts.Matching;
using MediatR;

namespace BaseFaq.AI.Business.Matching.Consumers;

public sealed class FaqMatchingRequestedConsumer(
    IMediator mediator) : MassTransit.IConsumer<FaqMatchingRequestedV1>
{
    public async Task Consume(MassTransit.ConsumeContext<FaqMatchingRequestedV1> context)
    {
        var message = context.Message;

        await mediator.Send(
            new ProcessFaqMatchingRequestedCommand(message),
            context.CancellationToken);
    }
}
