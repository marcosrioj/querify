using BaseFaq.Models.Common.Dtos;

namespace BaseFaq.Models.Faq.Dtos.Vote;

public class VoteGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? FaqItemAnswerId { get; set; }
}
