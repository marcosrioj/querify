using BaseFaq.Models.Common.Dtos;

namespace BaseFaq.Models.Faq.Dtos.FaqItemAnswer;

public class FaqItemAnswerGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? FaqItemId { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchText { get; set; }
}
