using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Models.Faq.Dtos.Faq;

public class FaqGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public FaqStatus? Status { get; set; }
    public List<Guid>? FaqIds { get; set; }
    public bool IncludeFaqItems { get; set; }
    public bool IncludeContentRefs { get; set; }
    public bool IncludeTags { get; set; }
}
