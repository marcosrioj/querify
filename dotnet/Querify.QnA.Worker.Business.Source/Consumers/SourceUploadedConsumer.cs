using MassTransit;
using Querify.Models.QnA.Dtos.IntegrationEvents;
using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Business.Source.Consumers;

public sealed class SourceUploadedConsumer(ISourceUploadVerificationService verificationService)
    : IConsumer<SourceUploadedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SourceUploadedIntegrationEvent> context)
    {
        await verificationService.VerifyUploadedAsync(context.Message, context.CancellationToken);
    }
}
