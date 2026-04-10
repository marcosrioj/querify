using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Worker.Business.Billing.Abstractions;
using BaseFaq.Tenant.Worker.Business.Billing.Models;

namespace BaseFaq.Tenant.Worker.Business.Billing.Services;

public sealed class StripeBillingProvider(StripeWebhookEventMapper mapper) : IBillingProvider
{
    public BillingProviderType Provider => BillingProviderType.Stripe;

    public BillingWebhookEvent Parse(BillingWebhookInbox workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        return mapper.Map(workItem);
    }
}
