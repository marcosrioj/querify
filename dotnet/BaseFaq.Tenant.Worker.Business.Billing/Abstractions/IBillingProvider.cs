using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Worker.Business.Billing.Models;

namespace BaseFaq.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingProvider
{
    BillingProviderType Provider { get; }

    BillingWebhookEvent Parse(BillingWebhookInbox workItem);
}
