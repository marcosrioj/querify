using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSourcesForAllTenants;
using Querify.QnA.Worker.Business.Source.Options;
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
    public async Task VerifyUploadedSourcesAsync_UsesConfiguredBatchSize()
    {
        var mediator = new CapturingMediator();
        var service = new SourceUploadVerificationSweepService(
            mediator,
            new TestOptionsMonitor<SourceUploadVerificationSweepOptions>(new SourceUploadVerificationSweepOptions
            {
                BatchSize = 7
            }));

        await service.VerifyUploadedSourcesAsync(CancellationToken.None);

        var command = Assert.IsType<VerifyUploadedSourcesForAllTenantsCommand>(mediator.LastRequest);
        Assert.Equal(7, command.BatchSize);
    }
}
