using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Models.Faq.Dtos.ContentRef;

public class ContentRefGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public ContentRefKind? Kind { get; set; }
    public Guid? FaqId { get; set; }
    public Guid? FaqItemId { get; set; }
}
