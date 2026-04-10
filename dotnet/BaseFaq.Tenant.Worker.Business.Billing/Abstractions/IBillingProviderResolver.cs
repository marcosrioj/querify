using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingProviderResolver
{
    IBillingProvider Resolve(BillingProviderType provider);
}
