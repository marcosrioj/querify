namespace BaseFaq.Models.QnA.Dtos.Topic;

public class TopicCreateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
}
