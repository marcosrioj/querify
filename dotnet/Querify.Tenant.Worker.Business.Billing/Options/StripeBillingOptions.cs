namespace Querify.Tenant.Worker.Business.Billing.Options;

public sealed class StripeBillingOptions
{
    public const string SectionName = "TenantWorker:Billing:Stripe";

    public string? ApiKey { get; set; }
    public string DefaultCurrency { get; set; } = "usd";
    public string? CheckoutSuccessUrl { get; set; }
    public string? CheckoutCancelUrl { get; set; }
    public string? BillingPortalReturnUrl { get; set; }
}
