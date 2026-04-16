namespace BaseFaq.Models.QnA.Dtos.QuestionSpace;

public class QuestionSpaceTopicDto
{
    public Guid Id { get; set; }
    public Guid QuestionSpaceId { get; set; }
    public Guid TopicId { get; set; }
}
