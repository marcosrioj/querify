using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Search;

public class QnASearchRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public Guid? SpaceId { get; set; }
    public string? SpaceSlug { get; set; }
    public QuestionStatus? Status { get; set; }
    public VisibilityScope? Visibility { get; set; }
}
