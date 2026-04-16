using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.ThreadActivity;

public class ThreadActivityGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? QuestionId { get; set; }
    public Guid? AnswerId { get; set; }
    public ActivityKind? Kind { get; set; }
    public ActorKind? ActorKind { get; set; }
}
