using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Abstractions;
using BaseFaq.Tenant.Worker.Business.Billing.Abstractions;
using BaseFaq.Tenant.Worker.Business.Billing.Models;
using BaseFaq.Tenant.Worker.Business.Billing.Services;

namespace BaseFaq.Tenant.Worker.Business.Billing.EventHandlers;

public sealed class CheckoutCompletedBillingWebhookEventHandler(
    TenantDbContext dbContext,
    BillingStateService billingStateService,
    ITenantEntitlementSynchronizer entitlementSynchronizer)
    : IBillingWebhookEventHandler
{
    public BillingWebhookEventKind Kind => BillingWebhookEventKind.CheckoutCompleted;

    public async Task HandleAsync(BillingWebhookEvent billingEvent, CancellationToken cancellationToken = default)
    {
        var tenantId = await billingStateService.ResolveRequiredTenantIdAsync(billingEvent, cancellationToken);

        await billingStateService.UpsertCustomerAsync(billingEvent, tenantId, cancellationToken);
        var subscription = await billingStateService.UpsertTenantSubscriptionAsync(
            billingEvent,
            tenantId,
            cancellationToken);

        await billingStateService.UpsertProviderSubscriptionAsync(
            billingEvent,
            tenantId,
            subscription.Id,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await entitlementSynchronizer.SynchronizeAsync(tenantId, cancellationToken);
    }
}
