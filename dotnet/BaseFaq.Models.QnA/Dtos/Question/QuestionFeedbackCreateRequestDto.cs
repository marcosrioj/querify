namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionFeedbackCreateRequestDto
{
    public Guid QuestionId { get; set; }
    public bool Like { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
