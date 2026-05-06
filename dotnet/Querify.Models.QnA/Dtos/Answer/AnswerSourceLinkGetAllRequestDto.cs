using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Answer;

public class AnswerSourceLinkGetAllRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? AnswerId { get; set; }
    public Guid? SourceId { get; set; }
    public SourceRole? Role { get; set; }
}
