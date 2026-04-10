using BaseFaq.Common.EntityFramework.Tenant;
using Microsoft.EntityFrameworkCore;
using BaseFaq.Tenant.Worker.Business.Billing.Models;

namespace BaseFaq.Tenant.Worker.Business.Billing.Services;

public sealed class BillingTenantResolver(TenantDbContext dbContext)
{
    public async Task<Guid?> ResolveTenantIdAsync(
        BillingWebhookEvent billingEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(billingEvent);

        if (billingEvent.TenantId.HasValue)
        {
            return billingEvent.TenantId.Value;
        }

        if (!string.IsNullOrWhiteSpace(billingEvent.ExternalCustomerId))
        {
            var tenantId = await dbContext.BillingCustomers
                .AsNoTracking()
                .Where(entry =>
                    entry.Provider == billingEvent.Provider &&
                    entry.ExternalCustomerId == billingEvent.ExternalCustomerId)
                .Select(entry => (Guid?)entry.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (tenantId.HasValue)
            {
                return tenantId.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(billingEvent.ExternalSubscriptionId))
        {
            var tenantId = await dbContext.BillingProviderSubscriptions
                .AsNoTracking()
                .Where(entry =>
                    entry.Provider == billingEvent.Provider &&
                    entry.ExternalSubscriptionId == billingEvent.ExternalSubscriptionId)
                .Select(entry => (Guid?)entry.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (tenantId.HasValue)
            {
                return tenantId.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(billingEvent.ExternalInvoiceId))
        {
            var tenantId = await dbContext.BillingInvoices
                .AsNoTracking()
                .Where(entry =>
                    entry.Provider == billingEvent.Provider &&
                    entry.ExternalInvoiceId == billingEvent.ExternalInvoiceId)
                .Select(entry => (Guid?)entry.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (tenantId.HasValue)
            {
                return tenantId.Value;
            }
        }

        return null;
    }
}
