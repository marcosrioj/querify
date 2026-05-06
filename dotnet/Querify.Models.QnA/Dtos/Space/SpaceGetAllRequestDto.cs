using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Space;

public class SpaceGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public VisibilityScope? Visibility { get; set; }
    public SpaceStatus? Status { get; set; }
    public bool? AcceptsQuestions { get; set; }
    public bool? AcceptsAnswers { get; set; }
}
