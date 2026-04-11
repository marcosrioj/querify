using BaseFaq.Common.EntityFramework.Tenant.Enums;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Public.Business.Billing.Commands.IngestStripeWebhook;
using BaseFaq.Tenant.Public.Business.Billing.Options;
using BaseFaq.Tenant.Public.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BaseFaq.Tenant.Public.Test.IntegrationTests.Tests.Billing;

public class IngestStripeWebhookCommandHandlerTests
{
    private const string TestWebhookSecret = "whsec_test_integration_secret_key_for_basefaq";

    private static IngestStripeWebhookCommandHandler CreateHandler(
        BaseFaq.Common.EntityFramework.Tenant.TenantDbContext dbContext)
    {
        var options = Options.Create(new StripeWebhookOptions { WebhookSecret = TestWebhookSecret });
        var logger = NullLogger<IngestStripeWebhookCommandHandler>.Instance;
        return new IngestStripeWebhookCommandHandler(dbContext, options, logger);
    }

    [Fact]
    public async Task ValidPayload_WithCorrectSignature_PersistsInboxRowAndReturnsTrue()
    {
        using var context = TestContext.Create();

        var eventId = $"evt_test_{Guid.NewGuid():N}";
        var payload = StripeTestHelper.BuildPayload(eventId, "checkout.session.completed");
        var signature = StripeTestHelper.ComputeSignature(TestWebhookSecret, payload);

        var handler = CreateHandler(context.DbContext);
        var result = await handler.Handle(
            new IngestStripeWebhookCommand { PayloadJson = payload, Signature = signature },
            CancellationToken.None);

        Assert.True(result);
        var stored = await context.DbContext.BillingWebhookInboxes
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.ExternalEventId == eventId);
        Assert.NotNull(stored);
        Assert.Equal("checkout.session.completed", stored!.EventType);
        Assert.Equal(BillingProviderType.Stripe, stored.Provider);
        Assert.Equal(ControlPlaneMessageStatus.Pending, stored.Status);
        Assert.True(stored.SignatureValid);
        Assert.Equal(0, stored.AttemptCount);
        Assert.Null(stored.TenantId);
    }

    [Fact]
    public async Task ValidPayload_WithTenantIdInMetadata_ExtractsTenantIdOntoInboxRow()
    {
        using var context = TestContext.Create();

        var tenantId = Guid.NewGuid();
        var eventId = $"evt_test_{Guid.NewGuid():N}";
        var payload = StripeTestHelper.BuildPayload(eventId, "customer.subscription.created", tenantId: tenantId);
        var signature = StripeTestHelper.ComputeSignature(TestWebhookSecret, payload);

        var handler = CreateHandler(context.DbContext);
        await handler.Handle(
            new IngestStripeWebhookCommand { PayloadJson = payload, Signature = signature },
            CancellationToken.None);

        var stored = await context.DbContext.BillingWebhookInboxes
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.ExternalEventId == eventId);
        Assert.NotNull(stored);
        Assert.Equal(tenantId, stored!.TenantId);
    }

    [Fact]
    public async Task NullSignature_ThrowsApiErrorException_BadRequest()
    {
        using var context = TestContext.Create();

        var payload = StripeTestHelper.BuildPayload("evt_nosig", "invoice.paid");

        var handler = CreateHandler(context.DbContext);
        var ex = await Assert.ThrowsAsync<ApiErrorException>(() =>
            handler.Handle(
                new IngestStripeWebhookCommand { PayloadJson = payload, Signature = null },
                CancellationToken.None));

        Assert.Equal(400, ex.ErrorCode);
    }

    [Fact]
    public async Task InvalidSignature_ThrowsApiErrorException_BadRequest()
    {
        using var context = TestContext.Create();

        var payload = StripeTestHelper.BuildPayload("evt_badsig", "invoice.paid");
        var wrongSignature = "t=1,v1=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        var handler = CreateHandler(context.DbContext);
        var ex = await Assert.ThrowsAsync<ApiErrorException>(() =>
            handler.Handle(
                new IngestStripeWebhookCommand { PayloadJson = payload, Signature = wrongSignature },
                CancellationToken.None));

        Assert.Equal(400, ex.ErrorCode);
    }

    [Fact]
    public async Task DuplicateEventId_SecondIngest_ReturnsFalseWithoutCreatingAnotherRow()
    {
        using var context = TestContext.Create();

        var eventId = $"evt_dup_{Guid.NewGuid():N}";
        var payload = StripeTestHelper.BuildPayload(eventId, "invoice.paid");
        var signature = StripeTestHelper.ComputeSignature(TestWebhookSecret, payload);

        var handler = CreateHandler(context.DbContext);
        var firstResult = await handler.Handle(
            new IngestStripeWebhookCommand { PayloadJson = payload, Signature = signature },
            CancellationToken.None);

        // Re-compute signature so the timestamp is still fresh
        signature = StripeTestHelper.ComputeSignature(TestWebhookSecret, payload);
        var secondResult = await handler.Handle(
            new IngestStripeWebhookCommand { PayloadJson = payload, Signature = signature },
            CancellationToken.None);

        Assert.True(firstResult);
        Assert.False(secondResult);

        var count = await context.DbContext.BillingWebhookInboxes
            .AsNoTracking()
            .CountAsync(e => e.ExternalEventId == eventId);
        Assert.Equal(1, count);
    }
}
