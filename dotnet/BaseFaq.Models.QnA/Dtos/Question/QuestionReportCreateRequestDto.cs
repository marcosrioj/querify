namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionReportCreateRequestDto
{
    public required Guid QuestionId { get; set; }
    public Guid? AnswerId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
