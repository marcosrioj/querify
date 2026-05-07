using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Tenant.Worker.Business.Email.Commands.ProcessEmailOutboxBatch;
using Querify.Tenant.Worker.Business.Email.Options;
using Querify.Tenant.Worker.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Querify.Tenant.Worker.Test.IntegrationTests.Tests.Email;

public class ProcessEmailOutboxBatchCommandHandlerTests
{
    private static EmailProcessingOptions DefaultOptions => new()
    {
        BatchSize = 10,
        LeaseDurationSeconds = 120,
        FailureBackoffSeconds = 60,
        MaxRetryCount = 3
    };

    private static ProcessEmailOutboxBatchCommandHandler CreateHandler(
        Querify.Common.EntityFramework.Tenant.TenantDbContext dbContext,
        FakeMediator mediator,
        EmailProcessingOptions? options = null)
    {
        var monitor = new TestOptionsMonitor<EmailProcessingOptions>(options ?? DefaultOptions);
        var logger = NullLogger<ProcessEmailOutboxBatchCommandHandler>.Instance;
        return new ProcessEmailOutboxBatchCommandHandler(dbContext, mediator, monitor, logger);
    }

    private static async Task<EmailOutbox> SeedPendingItemAsync(
        Querify.Common.EntityFramework.Tenant.TenantDbContext dbContext,
        int attemptCount = 0,
        DateTime? nextAttemptDateUtc = null,
        DateTime? lockedUntilDateUtc = null)
    {
        var item = new EmailOutbox
        {
            RecipientEmail = $"test_{Guid.NewGuid():N}@example.test",
            Subject = "Test email",
            HtmlBody = "<p>Test</p>",
            Status = ControlPlaneMessageStatus.Pending,
            AttemptCount = attemptCount,
            NextAttemptDateUtc = nextAttemptDateUtc,
            LockedUntilDateUtc = lockedUntilDateUtc
        };
        dbContext.EmailOutboxes.Add(item);
        await dbContext.SaveChangesAsync();
        return item;
    }

    // SendEmailOutboxCommandHandler always throws ApiErrorException (no provider implemented).
    // The processor catches it and schedules a retry, so there is no "success" path in the current codebase.

    [Fact]
    public async Task PendingItem_HandlerThrows_MarkedPendingWithRetryScheduled()
    {
        using var context = TestContext.Create();
        var seeded = await SeedPendingItemAsync(context.DbContext);

        // The real handler throws; the fake mediator simulates that.
        var mediator = new FakeMediator();
        mediator.EnqueueException(new ApiErrorException("No email provider is implemented yet."));
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessEmailOutboxBatchCommand(), CancellationToken.None);

        Assert.True(processedAny);

        var item = await context.DbContext.EmailOutboxes
            .AsNoTracking()
            .SingleAsync(e => e.Id == seeded.Id);
        Assert.Equal(ControlPlaneMessageStatus.Pending, item.Status);
        Assert.NotNull(item.NextAttemptDateUtc);
        Assert.Equal(1, item.AttemptCount);
        Assert.NotNull(item.LastError);
        Assert.Null(item.ProcessedDateUtc);
    }

    [Fact]
    public async Task PendingItem_MaxRetryExceeded_MarkedFailed()
    {
        using var context = TestContext.Create();
        var options = new EmailProcessingOptions
        {
            BatchSize = DefaultOptions.BatchSize,
            LeaseDurationSeconds = DefaultOptions.LeaseDurationSeconds,
            FailureBackoffSeconds = DefaultOptions.FailureBackoffSeconds,
            MaxRetryCount = 3
        };
        var seeded = await SeedPendingItemAsync(context.DbContext, attemptCount: 2);

        var mediator = new FakeMediator();
        mediator.EnqueueException(new ApiErrorException("No email provider is implemented yet."));
        var handler = CreateHandler(context.DbContext, mediator, options);

        await handler.Handle(new ProcessEmailOutboxBatchCommand(), CancellationToken.None);

        var item = await context.DbContext.EmailOutboxes
            .AsNoTracking()
            .SingleAsync(e => e.Id == seeded.Id);
        Assert.Equal(ControlPlaneMessageStatus.Failed, item.Status);
        Assert.Equal(3, item.AttemptCount);
        Assert.NotNull(item.LastError);
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

        var processedAny = await handler.Handle(new ProcessEmailOutboxBatchCommand(), CancellationToken.None);

        Assert.False(processedAny);
        Assert.Equal(0, mediator.SendCallCount);
    }

    [Fact]
    public async Task EmptyOutbox_ReturnsZero()
    {
        using var context = TestContext.Create();

        var mediator = new FakeMediator();
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessEmailOutboxBatchCommand(), CancellationToken.None);

        Assert.False(processedAny);
        Assert.Equal(0, mediator.SendCallCount);
    }

    [Fact]
    public async Task MediatorSucceeds_ItemMarkedCompleted()
    {
        using var context = TestContext.Create();
        var seeded = await SeedPendingItemAsync(context.DbContext);

        // FakeMediator does NOT throw → simulates a future successful provider
        var mediator = new FakeMediator();
        var handler = CreateHandler(context.DbContext, mediator);

        var processedAny = await handler.Handle(new ProcessEmailOutboxBatchCommand(), CancellationToken.None);

        Assert.True(processedAny);

        var item = await context.DbContext.EmailOutboxes
            .AsNoTracking()
            .SingleAsync(e => e.Id == seeded.Id);
        Assert.Equal(ControlPlaneMessageStatus.Completed, item.Status);
        Assert.NotNull(item.ProcessedDateUtc);
        Assert.Null(item.LastError);
    }
}
