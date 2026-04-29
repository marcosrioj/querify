using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Worker.Business.Billing.Abstractions;
using System.Net;

namespace BaseFaq.Tenant.Worker.Business.Billing.Services;

public sealed class BillingProviderResolver(IEnumerable<IBillingProvider> providers) : IBillingProviderResolver
{
    private readonly Dictionary<BillingProviderType, IBillingProvider> _providers = providers.ToDictionary(
        provider => provider.Provider,
        provider => provider);

    public IBillingProvider Resolve(BillingProviderType provider)
    {
        if (_providers.TryGetValue(provider, out var resolvedProvider))
        {
            return resolvedProvider;
        }

        throw new ApiErrorException(
            $"No billing provider is registered for '{provider}'.",
            (int)HttpStatusCode.InternalServerError);
    }
}
