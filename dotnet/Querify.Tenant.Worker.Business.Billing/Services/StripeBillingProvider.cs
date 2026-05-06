using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Worker.Business.Billing.Abstractions;
using Querify.Tenant.Worker.Business.Billing.Models;

namespace Querify.Tenant.Worker.Business.Billing.Services;

public sealed class StripeBillingProvider(StripeWebhookEventMapper mapper) : IBillingProvider
{
    public BillingProviderType Provider => BillingProviderType.Stripe;

    public BillingWebhookEvent Parse(BillingWebhookInbox workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        return mapper.Map(workItem);
    }
}
