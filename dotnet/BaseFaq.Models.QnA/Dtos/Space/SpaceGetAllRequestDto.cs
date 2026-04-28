using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public VisibilityScope? Visibility { get; set; }
    public SpaceStatus? Status { get; set; }
    public bool? AcceptsQuestions { get; set; }
    public bool? AcceptsAnswers { get; set; }
}
