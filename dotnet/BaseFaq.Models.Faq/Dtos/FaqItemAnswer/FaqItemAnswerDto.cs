namespace BaseFaq.Models.Faq.Dtos.FaqItemAnswer;

public class FaqItemAnswerDto
{
    public required Guid Id { get; set; }
    public required string ShortAnswer { get; set; }
    public string? Answer { get; set; }
    public required int Sort { get; set; }
    public required int VoteScore { get; set; }
    public required bool IsActive { get; set; }
    public required Guid FaqItemId { get; set; }
}
