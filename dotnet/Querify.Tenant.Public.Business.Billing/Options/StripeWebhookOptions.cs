using System.ComponentModel.DataAnnotations;

namespace Querify.Tenant.Public.Business.Billing.Options;

public sealed class StripeWebhookOptions
{
    public const string SectionName = "TenantPublic:Billing:Stripe";

    [Required]
    public string WebhookSecret { get; set; } = string.Empty;
}
