using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public Guid? SpaceId { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    public QuestionStatus? Status { get; set; }
    public VisibilityScope? Visibility { get; set; }
    public string? SpaceSlug { get; set; }
    public bool IncludeAnswers { get; set; }
    public bool IncludeTags { get; set; }
    public bool IncludeSources { get; set; }
    public bool IncludeActivity { get; set; }
}
