namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionFeedbackCreateRequestDto
{
    public required Guid QuestionId { get; set; }
    public required bool Like { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
