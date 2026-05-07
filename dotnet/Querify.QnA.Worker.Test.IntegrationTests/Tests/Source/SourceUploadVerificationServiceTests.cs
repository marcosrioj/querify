using Querify.Models.QnA.Dtos.IntegrationEvents;
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

        await verificationService.VerifyUploadedAsync(new SourceUploadedIntegrationEvent
        {
            TenantId = tenantId,
            SourceId = sourceId,
            StorageKey = $"{tenantId}/sources/{sourceId}/staging/manual.pdf",
            ClientChecksum = "sha256:test",
            UploadedAtUtc = DateTime.UtcNow
        }, CancellationToken.None);

        var command = Assert.IsType<VerifyUploadedSourceCommand>(mediator.LastRequest);
        Assert.Equal(tenantId, command.TenantId);
        Assert.Equal(sourceId, command.SourceId);
        Assert.Equal("sha256:test", command.ClientChecksum);
        Assert.Equal(Guid.Empty, tenantContext.TenantId);
    }
}
