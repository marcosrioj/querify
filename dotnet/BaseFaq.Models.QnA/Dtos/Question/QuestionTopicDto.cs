namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionTopicDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid TopicId { get; set; }
}
