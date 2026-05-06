using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.Worker.Business.Billing.Models;

namespace Querify.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingProvider
{
    BillingProviderType Provider { get; }

    BillingWebhookEvent Parse(BillingWebhookInbox workItem);
}
