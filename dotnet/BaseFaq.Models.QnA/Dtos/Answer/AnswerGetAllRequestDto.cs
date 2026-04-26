using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? QuestionId { get; set; }
    public AnswerStatus? Status { get; set; }
    public VisibilityScope? Visibility { get; set; }
    public bool? IsAccepted { get; set; }
}
