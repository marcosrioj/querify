namespace Querify.Models.Tenant.Dtos.Billing;

public sealed class BillingWebhookInboxDetailDto : BillingWebhookInboxDto
{
    public string PayloadJson { get; set; } = string.Empty;
    public string? Signature { get; set; }
}
