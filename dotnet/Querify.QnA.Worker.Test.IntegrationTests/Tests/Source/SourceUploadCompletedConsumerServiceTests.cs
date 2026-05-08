using Microsoft.Extensions.Logging.Abstractions;
using Querify.Models.QnA.Events;
using Querify.QnA.Worker.Api.Consumers;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Business.Source.Consumers;
using Querify.QnA.Worker.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Worker.Test.IntegrationTests.Tests.Source;

public class SourceUploadCompletedConsumerServiceTests
{
    [Fact]
    public async Task ProcessAsync_MapsEventToVerifyUploadedSourceCommand()
    {
        var tenantId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var tenantContext = new TestTenantContext();
        var mediator = new CapturingMediator();
        var consumerService = new SourceUploadCompletedConsumerService(tenantContext, mediator);

        var storageKey = $"{tenantId}/sources/{sourceId}/staging/manual.pdf";
        await consumerService.ProcessAsync(new SourceUploadCompletedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            TenantId = tenantId,
            SourceId = sourceId,
            StorageKey = storageKey,
            ContentType = "application/pdf",
            SizeBytes = 12,
            CompletedByUserId = Guid.NewGuid().ToString()
        }, CancellationToken.None);

        var command = Assert.IsType<VerifyUploadedSourceCommand>(mediator.LastRequest);
        Assert.Equal(tenantId, command.TenantId);
        Assert.Equal(sourceId, command.SourceId);
        Assert.Equal(storageKey, command.StorageKey);
        Assert.Equal(Guid.Empty, tenantContext.TenantId);
    }

    [Fact]
    public async Task SourceUploadCompletedConsumer_MapsMessageToConsumerService()
    {
        var tenantId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var storageKey = $"{tenantId}/sources/{sourceId}/staging/manual.pdf";
        var consumerService = new CapturingSourceUploadCompletedConsumerService();
        var consumer = new SourceUploadCompletedConsumer(
            consumerService,
            NullLogger<SourceUploadCompletedConsumer>.Instance);

        await consumer.HandleAsync(new SourceUploadCompletedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            TenantId = tenantId,
            SourceId = sourceId,
            StorageKey = storageKey,
            ContentType = "application/pdf",
            SizeBytes = 12,
            CompletedByUserId = Guid.NewGuid().ToString()
        }, CancellationToken.None);

        Assert.Equal(tenantId, consumerService.IntegrationEvent?.TenantId);
        Assert.Equal(sourceId, consumerService.IntegrationEvent?.SourceId);
        Assert.Equal(storageKey, consumerService.IntegrationEvent?.StorageKey);
        Assert.Equal(1, consumerService.CallCount);
    }

    private sealed class CapturingSourceUploadCompletedConsumerService : ISourceUploadCompletedConsumerService
    {
        public SourceUploadCompletedIntegrationEvent? IntegrationEvent { get; private set; }
        public int CallCount { get; private set; }

        public Task ProcessAsync(
            SourceUploadCompletedIntegrationEvent integrationEvent,
            CancellationToken cancellationToken)
        {
            IntegrationEvent = integrationEvent;
            CallCount++;
            return Task.CompletedTask;
        }
    }
}
