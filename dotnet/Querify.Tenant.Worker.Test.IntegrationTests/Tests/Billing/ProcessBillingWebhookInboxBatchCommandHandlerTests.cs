using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Worker.Business.Billing.Commands.ProcessBillingWebhookInboxBatch;
using Querify.Tenant.Worker.Business.Billing.Options;
using Querify.Tenant.Worker.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Querify.Tenant.Worker.Test.IntegrationTests.Tests.Billing;

public class ProcessBillingWebhookInboxBatchCommandHandlerTests
{
    private static BillingProcessingOptions DefaultOptions => new()
    {
        BatchSize = 10,
        LeaseDurationSeconds = 120,
        FailureBackoffSeconds = 60,
        MaxRetryCount = 3
    };

    private static ProcessBillingWebhookInboxBatchCommandHandler CreateHandler(
        Querify.Common.EntityFramework.Tenant.TenantDbContext dbContext,
        FakeMediator mediator,
        BillingProcessingOptions? options = null)
    {
        var monitor = new TestOptionsMonitor<BillingProcessingOptions>(options ?? DefaultOptions);
        var logger = NullLogger<ProcessBillingWebhookInboxBatchCommandHandler>.Instance;
        return new ProcessBillingWebhookInboxBatchCommandHandler(dbContext, mediator, monitor, logger);
    }

    private static async Task<BillingWebhookInbox> SeedPendingItemAsync(
        Querify.Common.EntityFramework.Tenant.TenantDbContext dbContext,
        int attemptCount = 0,
        DateTime? nextAttemptDateUtc = null,
        DateTime? lockedUntilDateUtc = null)
    {
        var item = new BillingWebhookInbox
        {
            Provider = BillingProviderType.Stripe,
            ExternalEventId = $"evt_test_{Guid.NewGuid():N}",
            EventType = "invoice.paid",
            PayloadJson = """{"id":"evt_1","type":"invoice.paid"}""",
            Status = ControlPlaneMessageStatus.Pending,
            AttemptCount = attemptCount,
            NextAttemptDateUtc = nextAttemptDateUtc,
            LockedUntilDateUtc = lockedUntilDateUtc
        };
        dbContext.BillingWebhookInboxes.Add(item);
        await dbContext.SaveChangesAsync();
        return item;
    }

    [Fact]
    public async Task PendingItem_MediatorSucceeds_MarkedCompletedAndReturnsOne()
    {
        using var context = TestContext.Create();
        var seeded = await SeedPendingItemAsync(context.DbContext);

        var mediator = new FakeMediator();
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessBillingWebhookInboxBatchCommand(), CancellationToken.None);

        Assert.True(processedAny);
        Assert.Equal(1, mediator.SendCallCount);

        var item = await context.DbContext.BillingWebhookInboxes
            .AsNoTracking()
            .SingleAsync(e => e.Id == seeded.Id);
        Assert.Equal(ControlPlaneMessageStatus.Completed, item.Status);
        Assert.NotNull(item.ProcessedDateUtc);
        Assert.Null(item.LastError);
        Assert.Null(item.LockedUntilDateUtc);
        Assert.Null(item.ProcessingToken);
    }

    [Fact]
    public async Task PendingItem_MediatorThrows_MarkedPendingWithRetryScheduled()
    {
        using var context = TestContext.Create();
        var seeded = await SeedPendingItemAsync(context.DbContext);

        var mediator = new FakeMediator();
        mediator.EnqueueException(new InvalidOperationException("transient error"));
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessBillingWebhookInboxBatchCommand(), CancellationToken.None);

        Assert.True(processedAny);

        var item = await context.DbContext.BillingWebhookInboxes
            .AsNoTracking()
            .SingleAsync(e => e.Id == seeded.Id);
        Assert.Equal(ControlPlaneMessageStatus.Pending, item.Status);
        Assert.NotNull(item.NextAttemptDateUtc);
        Assert.Equal("transient error", item.LastError);
        Assert.Null(item.ProcessedDateUtc);
        Assert.Equal(1, item.AttemptCount);
    }

    [Fact]
    public async Task PendingItem_MaxRetryExceeded_MarkedFailed()
    {
        using var context = TestContext.Create();
        var options = new BillingProcessingOptions
        {
            BatchSize = DefaultOptions.BatchSize,
            LeaseDurationSeconds = DefaultOptions.LeaseDurationSeconds,
            FailureBackoffSeconds = DefaultOptions.FailureBackoffSeconds,
            MaxRetryCount = 3
        };
        // AttemptCount=2 means after claiming (→3) it equals MaxRetryCount → terminal failure
        var seeded = await SeedPendingItemAsync(context.DbContext, attemptCount: 2);

        var mediator = new FakeMediator();
        mediator.EnqueueException(new InvalidOperationException("permanent error"));
        var handler = CreateHandler(context.DbContext, mediator, options);

        await handler.Handle(new ProcessBillingWebhookInboxBatchCommand(), CancellationToken.None);

        var item = await context.DbContext.BillingWebhookInboxes
            .AsNoTracking()
            .SingleAsync(e => e.Id == seeded.Id);
        Assert.Equal(ControlPlaneMessageStatus.Failed, item.Status);
        Assert.Equal("permanent error", item.LastError);
        Assert.Null(item.ProcessedDateUtc);
        Assert.Equal(3, item.AttemptCount);
    }

    [Fact]
    public async Task LockedItem_LeaseActive_NotClaimedAndReturnsZero()
    {
        using var context = TestContext.Create();
        await SeedPendingItemAsync(
            context.DbContext,
            lockedUntilDateUtc: DateTime.UtcNow.AddHours(1));

        var mediator = new FakeMediator();
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessBillingWebhookInboxBatchCommand(), CancellationToken.None);

        Assert.False(processedAny);
        Assert.Equal(0, mediator.SendCallCount);
    }

    [Fact]
    public async Task ItemScheduledFuture_NextAttemptNotElapsed_NotClaimedAndReturnsZero()
    {
        using var context = TestContext.Create();
        await SeedPendingItemAsync(
            context.DbContext,
            nextAttemptDateUtc: DateTime.UtcNow.AddHours(1));

        var mediator = new FakeMediator();
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessBillingWebhookInboxBatchCommand(), CancellationToken.None);

        Assert.False(processedAny);
    }

    [Fact]
    public async Task EmptyInbox_ReturnsZero()
    {
        using var context = TestContext.Create();

        var mediator = new FakeMediator();
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessBillingWebhookInboxBatchCommand(), CancellationToken.None);

        Assert.False(processedAny);
        Assert.Equal(0, mediator.SendCallCount);
    }

    [Fact]
    public async Task BatchSize_LimitsItemsClaimed()
    {
        using var context = TestContext.Create();
        for (var i = 0; i < 5; i++)
        {
            await SeedPendingItemAsync(context.DbContext);
        }

        var options = new BillingProcessingOptions
        {
            BatchSize = 2,
            LeaseDurationSeconds = DefaultOptions.LeaseDurationSeconds,
            FailureBackoffSeconds = DefaultOptions.FailureBackoffSeconds,
            MaxRetryCount = DefaultOptions.MaxRetryCount
        };
        var mediator = new FakeMediator();
        var handler = CreateHandler(context.DbContext, mediator, options);

        var processedAny = await handler.Handle(new ProcessBillingWebhookInboxBatchCommand(), CancellationToken.None);

        Assert.True(processedAny);
        Assert.Equal(2, mediator.SendCallCount);
    }
}
