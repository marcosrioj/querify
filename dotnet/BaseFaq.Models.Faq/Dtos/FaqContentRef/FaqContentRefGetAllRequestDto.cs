using BaseFaq.Models.Common.Dtos;

namespace BaseFaq.Models.Faq.Dtos.FaqContentRef;

public class FaqContentRefGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? FaqId { get; set; }
    public Guid? ContentRefId { get; set; }
}
