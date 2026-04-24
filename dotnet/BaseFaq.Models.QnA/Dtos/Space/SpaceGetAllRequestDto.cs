using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public VisibilityScope? Visibility { get; set; }
    public SpaceKind? Kind { get; set; }
    public QnAProductSurface? ProductSurface { get; set; }
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public bool? AcceptsQuestions { get; set; }
    public bool? AcceptsAnswers { get; set; }
}
