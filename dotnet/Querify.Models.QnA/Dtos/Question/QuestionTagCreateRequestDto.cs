namespace Querify.Models.QnA.Dtos.Question;

public class QuestionTagCreateRequestDto
{
    public required Guid QuestionId { get; set; }
    public required Guid TagId { get; set; }
}
