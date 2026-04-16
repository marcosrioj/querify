using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerSourceLinkDto
{
    public Guid Id { get; set; }
    public Guid AnswerId { get; set; }
    public Guid SourceId { get; set; }
    public SourceRole Role { get; set; }
    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }
    public BaseFaq.Models.QnA.Dtos.KnowledgeSource.KnowledgeSourceDto? Source { get; set; }
}
