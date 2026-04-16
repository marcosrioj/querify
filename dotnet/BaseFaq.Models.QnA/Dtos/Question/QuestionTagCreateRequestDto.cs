namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionTagCreateRequestDto
{
    public Guid QuestionId { get; set; }
    public Guid TagId { get; set; }
}
