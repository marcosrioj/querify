using Querify.Models.Common.Dtos;

namespace Querify.Models.QnA.Dtos.Source;

public class SourceGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
}
