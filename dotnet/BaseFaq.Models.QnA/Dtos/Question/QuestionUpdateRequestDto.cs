namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionUpdateRequestDto : QuestionCreateRequestDto
{
    public Guid? AcceptedAnswerId { get; set; }
    public Guid? DuplicateOfQuestionId { get; set; }
}
