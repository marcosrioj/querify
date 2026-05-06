using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.Billing;

public sealed class BillingWebhookInboxGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? TenantId { get; set; }
    public BillingProviderType? Provider { get; set; }
    public string? Status { get; set; }
    public string? EventType { get; set; }
    public DateTime? ReceivedFromUtc { get; set; }
    public DateTime? ReceivedToUtc { get; set; }
}
