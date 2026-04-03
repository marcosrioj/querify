using BaseFaq.Models.Common.Dtos;

namespace BaseFaq.Models.Faq.Dtos.FaqItem;

public class FaqItemGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public Guid? FaqId { get; set; }
    public Guid? ContentRefId { get; set; }
    public bool? IsActive { get; set; }
}
