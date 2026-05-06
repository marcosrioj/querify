using Querify.Common.Infrastructure.MassTransit.Models;
using MassTransit;

namespace Querify.Common.Infrastructure.MassTransit.Consumers;

public abstract class BaseConsumer<TMessage>(IPublishEndpoint publishEndpoint) : IConsumer<TMessage>
    where TMessage : class
{
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        try
        {
            await HandleMessage(context.Message);
        }
        catch (Exception ex)
        {
            await HandleError(context, ex);
        }
    }

    protected abstract Task HandleMessage(TMessage message);

    private async Task HandleError(ConsumeContext<TMessage> context, Exception exception)
    {
        var errorEvent = new ErrorEvent
        {
            OriginalQueueName = context.ReceiveContext.InputAddress.AbsolutePath,
            ErrorQueueName = $"{context.ReceiveContext.InputAddress.AbsolutePath}_error",
            MessageId = context.MessageId.ToString(),
            ExceptionMessage = exception.Message,
            ExceptionStackTrace = exception.StackTrace,
            Timestamp = DateTime.UtcNow,
            CorrelationId = context.CorrelationId
        };

        await publishEndpoint.Publish(errorEvent);
    }
}