using System.ComponentModel.DataAnnotations;

namespace BaseFaq.Tenant.Public.Business.Billing.Options;

public sealed class StripeWebhookOptions
{
    public const string SectionName = "TenantPublic:Billing:Stripe";

    [Required]
    public string WebhookSecret { get; set; } = string.Empty;
}
