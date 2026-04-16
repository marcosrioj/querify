namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionTagDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid TagId { get; set; }
}
