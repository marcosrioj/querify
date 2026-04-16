using BaseFaq.Models.Common.Dtos;

namespace BaseFaq.Models.QnA.Dtos.Tag;

public class TagGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
}
