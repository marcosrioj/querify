using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Answer;

public class AnswerSourceLinkDto
{
    public required Guid Id { get; set; }
    public required Guid AnswerId { get; set; }
    public required Guid SourceId { get; set; }
    public required SourceRole Role { get; set; }
    public required int Order { get; set; }
    public Querify.Models.QnA.Dtos.Source.SourceDto? Source { get; set; }
}
