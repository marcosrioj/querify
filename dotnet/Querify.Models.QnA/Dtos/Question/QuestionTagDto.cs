namespace Querify.Models.QnA.Dtos.Question;

public class QuestionTagDto
{
    public required Guid Id { get; set; }
    public required Guid QuestionId { get; set; }
    public required Guid TagId { get; set; }
}
