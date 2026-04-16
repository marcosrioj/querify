using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerSourceLinkGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? AnswerId { get; set; }
    public Guid? SourceId { get; set; }
    public SourceRole? Role { get; set; }
}
