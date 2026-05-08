using Microsoft.Extensions.Logging.Abstractions;
using Querify.Models.QnA.Events;
using Querify.QnA.Worker.Api.Consumers;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Business.Source.Services;
using Querify.QnA.Worker.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Worker.Test.IntegrationTests.Tests.Source;

public class SourceUploadVerificationServiceTests
{
    [Fact]
    public async Task VerifyUploadedAsync_MapsEventToVerifyUploadedSourceCommand()
    {
        var tenantId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var tenantContext = new TestTenantContext();
        var mediator = new CapturingMediator();
        var verificationService = new SourceUploadVerificationService(tenantContext, mediator);

        var storageKey = $"{tenantId}/sources/{sourceId}/staging/manual.pdf";
        await verificationService.VerifyUploadedAsync(tenantId, sourceId, storageKey, CancellationToken.None);

        var command = Assert.IsType<VerifyUploadedSourceCommand>(mediator.LastRequest);
        Assert.Equal(tenantId, command.TenantId);
        Assert.Equal(sourceId, command.SourceId);
        Assert.Equal(storageKey, command.StorageKey);
        Assert.Equal(Guid.Empty, tenantContext.TenantId);
    }

    [Fact]
    public async Task SourceUploadCompletedConsumer_MapsMessageToVerificationService()
    {
        var tenantId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var storageKey = $"{tenantId}/sources/{sourceId}/staging/manual.pdf";
        var verificationService = new CapturingSourceUploadVerificationService();
        var consumer = new SourceUploadCompletedConsumer(
            verificationService,
            NullLogger<SourceUploadCompletedConsumer>.Instance);

        await consumer.HandleAsync(new SourceUploadCompletedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            TenantId = tenantId,
            SourceId = sourceId,
            StorageKey = storageKey,
            ClientChecksum = "sha256:client",
            ContentType = "application/pdf",
            SizeBytes = 12,
            CompletedByUserId = Guid.NewGuid().ToString()
        }, CancellationToken.None);

        Assert.Equal(tenantId, verificationService.TenantId);
        Assert.Equal(sourceId, verificationService.SourceId);
        Assert.Equal(storageKey, verificationService.StorageKey);
        Assert.Equal(1, verificationService.CallCount);
    }

    private sealed class CapturingSourceUploadVerificationService : ISourceUploadVerificationService
    {
        public Guid TenantId { get; private set; }
        public Guid SourceId { get; private set; }
        public string? StorageKey { get; private set; }
        public int CallCount { get; private set; }

        public Task VerifyUploadedAsync(
            Guid tenantId,
            Guid sourceId,
            string storageKey,
            CancellationToken cancellationToken)
        {
            TenantId = tenantId;
            SourceId = sourceId;
            StorageKey = storageKey;
            CallCount++;
            return Task.CompletedTask;
        }
    }
}
