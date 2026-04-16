using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Source;

public class SourceGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public SourceKind? Kind { get; set; }
    public VisibilityScope? Visibility { get; set; }
    public bool? IsAuthoritative { get; set; }
    public string? SystemName { get; set; }
}
