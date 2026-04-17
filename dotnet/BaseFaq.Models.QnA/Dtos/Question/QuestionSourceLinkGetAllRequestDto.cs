using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionSourceLinkGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? QuestionId { get; set; }
    public Guid? SourceId { get; set; }
    public SourceRole? Role { get; set; }
}
