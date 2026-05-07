using MassTransit;
using MediatR;
using Querify.Models.QnA.Dtos.IntegrationEvents;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;

namespace Querify.QnA.Worker.Business.Source.Consumers;

public sealed class SourceUploadedConsumer(
    IQnAWorkerTenantContext tenantContext,
    IMediator mediator)
    : IConsumer<SourceUploadedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SourceUploadedIntegrationEvent> context)
    {
        await HandleAsync(context.Message, context.CancellationToken);
    }

    public async Task HandleAsync(SourceUploadedIntegrationEvent message, CancellationToken cancellationToken)
    {
        using var tenantScope = tenantContext.UseTenant(message.TenantId);
        await mediator.Send(new VerifyUploadedSourceCommand
        {
            TenantId = message.TenantId,
            SourceId = message.SourceId,
            StorageKey = message.StorageKey,
            ClientChecksum = message.ClientChecksum,
            UploadedAtUtc = message.UploadedAtUtc
        }, cancellationToken);
    }
}
