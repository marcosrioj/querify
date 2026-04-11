using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingInvoices;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingPayments;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingSubscription;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingSummary;
using BaseFaq.Tenant.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.Tenant.Portal.Test.IntegrationTests.Tests.Billing;

public class BillingCommandQueryTests
{
    [Fact]
    public async Task GetBillingSummary_TenantWithActiveSubscription_ReturnsSummary()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);

        var now = DateTime.UtcNow;
        context.DbContext.TenantSubscriptions.Add(new TenantSubscription
        {
            TenantId = tenant.Id,
            PlanCode = "pro-monthly",
            Status = TenantSubscriptionStatus.Active,
            DefaultProvider = BillingProviderType.Stripe,
            CurrentPeriodStartUtc = now.AddDays(-15),
            CurrentPeriodEndUtc = now.AddDays(15)
        });

        context.DbContext.TenantEntitlementSnapshots.Add(new TenantEntitlementSnapshot
        {
            TenantId = tenant.Id,
            PlanCode = "pro-monthly",
            SubscriptionStatus = TenantSubscriptionStatus.Active,
            IsActive = true
        });

        context.DbContext.BillingInvoices.Add(new BillingInvoice
        {
            TenantId = tenant.Id,
            Provider = BillingProviderType.Stripe,
            ExternalInvoiceId = $"in_summary_{Guid.NewGuid():N}",
            AmountMinor = 2999,
            Currency = "usd",
            Status = BillingInvoiceStatus.Paid,
            PaidAtUtc = now.AddDays(-1)
        });

        context.DbContext.BillingPayments.Add(new BillingPayment
        {
            TenantId = tenant.Id,
            Provider = BillingProviderType.Stripe,
            ExternalPaymentId = $"pay_summary_{Guid.NewGuid():N}",
            AmountMinor = 2999,
            Currency = "usd",
            Status = BillingPaymentStatus.Succeeded,
            PaidAtUtc = now.AddDays(-1)
        });

        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingSummaryQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingSummaryQuery { TenantId = tenant.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenant.Id, result!.TenantId);
        Assert.Equal("pro-monthly", result.CurrentPlanCode);
        Assert.Equal(TenantSubscriptionStatus.Active, result.SubscriptionStatus);
        Assert.NotNull(result.LastInvoice);
        Assert.NotNull(result.LastPayment);
        Assert.NotNull(result.Entitlement);
        Assert.True(result.Entitlement!.IsActive);
    }

    [Fact]
    public async Task GetBillingSubscription_ReturnsProviderSubscriptions()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);

        var subscriptionId = Guid.NewGuid();
        context.DbContext.TenantSubscriptions.Add(new TenantSubscription
        {
            Id = subscriptionId,
            TenantId = tenant.Id,
            PlanCode = "starter-yearly",
            BillingInterval = BillingIntervalType.Year,
            Status = TenantSubscriptionStatus.Trialing,
            DefaultProvider = BillingProviderType.Stripe
        });

        context.DbContext.BillingProviderSubscriptions.Add(new BillingProviderSubscription
        {
            TenantId = tenant.Id,
            TenantSubscriptionId = subscriptionId,
            Provider = BillingProviderType.Stripe,
            ExternalSubscriptionId = $"sub_{Guid.NewGuid():N}",
            Status = TenantSubscriptionStatus.Trialing
        });

        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingSubscriptionQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingSubscriptionQuery { TenantId = tenant.Id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("starter-yearly", result!.PlanCode);
        Assert.Equal(BillingIntervalType.Year, result.BillingInterval);
        Assert.Single(result.ProviderSubscriptions);
        Assert.Equal(BillingProviderType.Stripe, result.ProviderSubscriptions[0].Provider);
    }

    [Fact]
    public async Task GetBillingInvoices_ReturnsOnlyCurrentTenantItems()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);
        var otherTenant = await TestDataFactory.SeedTenantAsync(context.DbContext, userId: Guid.NewGuid());

        context.DbContext.BillingInvoices.Add(new BillingInvoice
        {
            TenantId = tenant.Id,
            Provider = BillingProviderType.Stripe,
            ExternalInvoiceId = $"in_portal_{Guid.NewGuid():N}",
            AmountMinor = 1200,
            Currency = "usd",
            Status = BillingInvoiceStatus.Paid
        });
        context.DbContext.BillingInvoices.Add(new BillingInvoice
        {
            TenantId = otherTenant.Id,
            Provider = BillingProviderType.Stripe,
            ExternalInvoiceId = $"in_other_{Guid.NewGuid():N}",
            AmountMinor = 1800,
            Currency = "usd",
            Status = BillingInvoiceStatus.Open
        });

        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingInvoicesQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingInvoicesQuery
            {
                TenantId = tenant.Id,
                Request = new Models.Tenant.Dtos.Billing.BillingInvoiceGetAllRequestDto
                {
                    MaxResultCount = 50
                }
            },
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(tenant.Id, result.Items[0].TenantId);
    }

    [Fact]
    public async Task GetBillingPayments_ReturnsOnlyCurrentTenantItems()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext);
        var otherTenant = await TestDataFactory.SeedTenantAsync(context.DbContext, userId: Guid.NewGuid());

        context.DbContext.BillingPayments.Add(new BillingPayment
        {
            TenantId = tenant.Id,
            Provider = BillingProviderType.Stripe,
            ExternalPaymentId = $"pay_portal_{Guid.NewGuid():N}",
            AmountMinor = 4500,
            Currency = "usd",
            Status = BillingPaymentStatus.Succeeded
        });
        context.DbContext.BillingPayments.Add(new BillingPayment
        {
            TenantId = otherTenant.Id,
            Provider = BillingProviderType.Stripe,
            ExternalPaymentId = $"pay_other_{Guid.NewGuid():N}",
            AmountMinor = 5200,
            Currency = "usd",
            Status = BillingPaymentStatus.Pending
        });

        await context.DbContext.SaveChangesAsync();

        var handler = new GetBillingPaymentsQueryHandler(context.DbContext);
        var result = await handler.Handle(
            new GetBillingPaymentsQuery
            {
                TenantId = tenant.Id,
                Request = new Models.Tenant.Dtos.Billing.BillingPaymentGetAllRequestDto
                {
                    MaxResultCount = 50
                }
            },
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(tenant.Id, result.Items[0].TenantId);
    }
}
