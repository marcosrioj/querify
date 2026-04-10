using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.Billing;

public sealed class BillingWebhookInboxGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? TenantId { get; set; }
    public BillingProviderType? Provider { get; set; }
    public string? Status { get; set; }
    public string? EventType { get; set; }
    public DateTime? ReceivedFromUtc { get; set; }
    public DateTime? ReceivedToUtc { get; set; }
}
