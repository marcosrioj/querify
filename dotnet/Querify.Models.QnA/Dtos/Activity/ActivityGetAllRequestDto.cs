using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Activity;

public class ActivityGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public string? SearchText { get; set; }
    public Guid? SpaceId { get; set; }
    public Guid? QuestionId { get; set; }
    public Guid? AnswerId { get; set; }
    public ActivityKind? Kind { get; set; }
    public ActorKind? ActorKind { get; set; }
}
