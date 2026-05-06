using Querify.Models.Common.Dtos;

namespace Querify.Models.QnA.Dtos.Tag;

public class TagGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
}
