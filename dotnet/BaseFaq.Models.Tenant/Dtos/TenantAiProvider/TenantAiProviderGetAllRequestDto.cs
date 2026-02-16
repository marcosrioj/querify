using BaseFaq.Models.Common.Dtos;

namespace BaseFaq.Models.Tenant.Dtos.TenantAiProvider;

public class TenantAiProviderGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? TenantId { get; set; }
}