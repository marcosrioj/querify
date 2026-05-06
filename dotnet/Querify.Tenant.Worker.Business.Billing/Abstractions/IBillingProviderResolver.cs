using Querify.Models.Tenant.Enums;

namespace Querify.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingProviderResolver
{
    IBillingProvider Resolve(BillingProviderType provider);
}
