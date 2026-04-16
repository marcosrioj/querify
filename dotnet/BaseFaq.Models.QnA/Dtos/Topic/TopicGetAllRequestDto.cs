using BaseFaq.Models.Common.Dtos;

namespace BaseFaq.Models.QnA.Dtos.Topic;

public class TopicGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public string? Category { get; set; }
}
