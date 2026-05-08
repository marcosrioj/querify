using MassTransit;
using Querify.Models.QnA.Events;
using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Api.Consumers;

public sealed class SourceUploadCompletedConsumer(
    ISourceUploadVerificationService verificationService,
    ILogger<SourceUploadCompletedConsumer> logger)
    : IConsumer<SourceUploadCompletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SourceUploadCompletedIntegrationEvent> context)
    {
        await HandleAsync(context.Message, context.CancellationToken);
    }

    public async Task HandleAsync(
        SourceUploadCompletedIntegrationEvent message,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Verifying uploaded source {SourceId} for tenant {TenantId} from storage key {StorageKey}.",
            message.SourceId,
            message.TenantId,
            message.StorageKey);

        await verificationService.VerifyUploadedAsync(
            message.TenantId,
            message.SourceId,
            message.StorageKey,
            cancellationToken);
    }
}
