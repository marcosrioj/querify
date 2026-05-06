using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Question;

public class QuestionSourceLinkGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? QuestionId { get; set; }
    public Guid? SourceId { get; set; }
    public SourceRole? Role { get; set; }
}
