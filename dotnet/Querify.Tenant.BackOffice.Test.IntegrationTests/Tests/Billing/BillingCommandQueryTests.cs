using Querify.Common.EntityFramework.Tenant.Abstractions;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Common.EntityFramework.Tenant.Services;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.BackOffice.Business.Billing.Commands.RecomputeTenantEntitlements;
using Querify.Tenant.BackOffice.Business.Billing.Commands.RequeueBillingWebhookInbox;
using Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingInvoices;
using Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingSummary;
using Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInboxList;
using Querify.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Querify.Tenant.BackOffice.Test.IntegrationTests.Tests.Billing;

public class BillingCommandQueryTests
{
    // ───────────────────────── GetBillingSummary ──────────────────────────

    [Fact]
    public async Task GetBillingSummary_TenantWithActiveSubscription_ReturnsSummary()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);

        var now = DateTime.UtcNow;
        var subscription = new TenantSubscription
        {
            TenantId = tenant.Id,
            PlanCode = "pro-monthly",
            Status = TenantSubscriptionStatus.Active,
            DefaultProvider = BillingProviderType.Stripe,
            CurrentPeriodStartUtc = now.AddDays(-15),
            CurrentPeriodEndUtc = now.AddDays(15)
        };
        context.DbContext.TenantSubscriptions.Add(subscription);

        var entitlement = new TenantEntitlementSnapshot
        {
            TenantId = tenant.Id,
            PlanCode = "pro-monthly",
            SubscriptionStatus = TenantSubscriptionStatus.Active,
            IsActive = true
        };
        context.DbContext.TenantEntitlementSnapshots.Add(entitlement);
        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingSummaryQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingSummaryQuery { TenantId = tenant.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenant.Id, result!.TenantId);
        Assert.Equal("pro-monthly", result.CurrentPlanCode);
        Assert.Equal(TenantSubscriptionStatus.Active, result.SubscriptionStatus);
        Assert.NotNull(result.Entitlement);
        Assert.True(result.Entitlement!.IsActive);
    }

    [Fact]
    public async Task GetBillingSummary_NonExistentTenant_ReturnsNull()
    {
        using var context = TestContext.Create();

        var handler = new GetBillingSummaryQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingSummaryQuery { TenantId = Guid.NewGuid() },
            CancellationToken.None);

        Assert.Null(result);
    }

    // ─────────────────── GetBillingWebhookInboxList ───────────────────────

    [Fact]
    public async Task GetBillingWebhookInboxList_NoFilters_ReturnsAllItems()
    {
        using var context = TestContext.Create();

        for (var i = 0; i < 3; i++)
        {
            context.DbContext.BillingWebhookInboxes.Add(new BillingWebhookInbox
            {
                Provider = BillingProviderType.Stripe,
                ExternalEventId = $"evt_list_{i}_{Guid.NewGuid():N}",
                EventType = "invoice.paid",
                PayloadJson = "{}",
                Status = ControlPlaneMessageStatus.Pending
            });
        }
        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingWebhookInboxListQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingWebhookInboxListQuery
            {
                Request = new BillingWebhookInboxGetAllRequestDto
                {
                    MaxResultCount = 50
                }
            },
            CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetBillingWebhookInboxList_FilterByStatus_ReturnsMatchingItems()
    {
        using var context = TestContext.Create();

        context.DbContext.BillingWebhookInboxes.Add(new BillingWebhookInbox
        {
            Provider = BillingProviderType.Stripe,
            ExternalEventId = $"evt_pending_{Guid.NewGuid():N}",
            EventType = "invoice.paid",
            PayloadJson = "{}",
            Status = ControlPlaneMessageStatus.Pending
        });
        context.DbContext.BillingWebhookInboxes.Add(new BillingWebhookInbox
        {
            Provider = BillingProviderType.Stripe,
            ExternalEventId = $"evt_completed_{Guid.NewGuid():N}",
            EventType = "invoice.paid",
            PayloadJson = "{}",
            Status = ControlPlaneMessageStatus.Completed
        });
        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingWebhookInboxListQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingWebhookInboxListQuery
            {
                Request = new BillingWebhookInboxGetAllRequestDto
                {
                    Status = "Pending",
                    MaxResultCount = 50
                }
            },
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal("Pending", item.Status));
    }

    // ─────────────────────── GetBillingInvoices ───────────────────────────

    [Fact]
    public async Task GetBillingInvoices_ForTenant_ReturnsPaged()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);

        for (var i = 0; i < 3; i++)
        {
            context.DbContext.BillingInvoices.Add(new BillingInvoice
            {
                TenantId = tenant.Id,
                Provider = BillingProviderType.Stripe,
                ExternalInvoiceId = $"in_test_{i}_{Guid.NewGuid():N}",
                AmountMinor = 2999,
                Currency = "usd",
                Status = BillingInvoiceStatus.Paid,
                PaidAtUtc = DateTime.UtcNow.AddDays(-i)
            });
        }
        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingInvoicesQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingInvoicesQuery
            {
                TenantId = tenant.Id,
                Request = new BillingInvoiceGetAllRequestDto { MaxResultCount = 50 }
            },
            CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetBillingInvoices_OtherTenant_ReturnsEmpty()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);

        context.DbContext.BillingInvoices.Add(new BillingInvoice
        {
            TenantId = tenant.Id,
            Provider = BillingProviderType.Stripe,
            ExternalInvoiceId = $"in_other_{Guid.NewGuid():N}",
            AmountMinor = 1000,
            Currency = "usd",
            Status = BillingInvoiceStatus.Paid
        });
        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingInvoicesQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingInvoicesQuery
            {
                TenantId = Guid.NewGuid(),
                Request = new BillingInvoiceGetAllRequestDto { MaxResultCount = 50 }
            },
            CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
    }

    // ─────────────────── RequeueBillingWebhookInbox ───────────────────────

    [Fact]
    public async Task RequeueBillingWebhookInbox_FailedItem_ResetsToReadyState()
    {
        using var context = TestContext.Create();

        var item = new BillingWebhookInbox
        {
            Provider = BillingProviderType.Stripe,
            ExternalEventId = $"evt_requeue_{Guid.NewGuid():N}",
            EventType = "invoice.payment_failed",
            PayloadJson = "{}",
            Status = ControlPlaneMessageStatus.Failed,
            AttemptCount = 5,
            LastError = "card_declined",
            LastAttemptDateUtc = DateTime.UtcNow.AddMinutes(-30)
        };
        context.DbContext.BillingWebhookInboxes.Add(item);
        await context.DbContext.SaveChangesAsync();

        var handler = new RequeueBillingWebhookInboxCommandHandler(context.DbContext);
        var resultId = await handler.Handle(
            new RequeueBillingWebhookInboxCommand { Id = item.Id },
            CancellationToken.None);

        Assert.Equal(item.Id, resultId);

        var updated = await context.DbContext.BillingWebhookInboxes
            .AsNoTracking()
            .SingleAsync(e => e.Id == item.Id);
        Assert.Equal(ControlPlaneMessageStatus.Pending, updated.Status);
        Assert.Equal(0, updated.AttemptCount);
        Assert.Null(updated.LastError);
        Assert.Null(updated.ProcessedDateUtc);
        Assert.NotNull(updated.NextAttemptDateUtc);
    }

    [Fact]
    public async Task RequeueBillingWebhookInbox_NotFound_ThrowsApiErrorException404()
    {
        using var context = TestContext.Create();

        var handler = new RequeueBillingWebhookInboxCommandHandler(context.DbContext);
        var ex = await Assert.ThrowsAsync<ApiErrorException>(() =>
            handler.Handle(
                new RequeueBillingWebhookInboxCommand { Id = Guid.NewGuid() },
                CancellationToken.None));

        Assert.Equal(404, ex.ErrorCode);
    }

    // ─────────────────── RecomputeTenantEntitlements ──────────────────────

    [Fact]
    public async Task RecomputeTenantEntitlements_ActiveSubscription_CreatesActiveEntitlement()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);

        var now = DateTime.UtcNow;
        context.DbContext.TenantSubscriptions.Add(new TenantSubscription
        {
            TenantId = tenant.Id,
            PlanCode = "starter-monthly",
            Status = TenantSubscriptionStatus.Active,
            DefaultProvider = BillingProviderType.Stripe,
            CurrentPeriodStartUtc = now.AddDays(-10),
            CurrentPeriodEndUtc = now.AddDays(20)
        });
        await context.DbContext.SaveChangesAsync();

        ITenantEntitlementSynchronizer synchronizer = new TenantEntitlementSynchronizer(context.DbContext);
        var handler = new RecomputeTenantEntitlementsCommandHandler(context.DbContext, synchronizer);

        var resultId = await handler.Handle(
            new RecomputeTenantEntitlementsCommand { TenantId = tenant.Id },
            CancellationToken.None);

        Assert.Equal(tenant.Id, resultId);

        var snapshot = await context.DbContext.TenantEntitlementSnapshots
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.TenantId == tenant.Id);
        Assert.NotNull(snapshot);
        Assert.True(snapshot!.IsActive);
        Assert.Equal("starter-monthly", snapshot.PlanCode);
        Assert.Equal(TenantSubscriptionStatus.Active, snapshot.SubscriptionStatus);
    }

    [Fact]
    public async Task RecomputeTenantEntitlements_NonExistentTenant_ThrowsApiErrorException404()
    {
        using var context = TestContext.Create();

        ITenantEntitlementSynchronizer synchronizer = new TenantEntitlementSynchronizer(context.DbContext);
        var handler = new RecomputeTenantEntitlementsCommandHandler(context.DbContext, synchronizer);

        var ex = await Assert.ThrowsAsync<ApiErrorException>(() =>
            handler.Handle(
                new RecomputeTenantEntitlementsCommand { TenantId = Guid.NewGuid() },
                CancellationToken.None));

        Assert.Equal(404, ex.ErrorCode);
    }
}
