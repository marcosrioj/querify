namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerVoteCreateRequestDto
{
    public Guid QuestionId { get; set; }
    public Guid AnswerId { get; set; }
    public bool IsUpvote { get; set; } = true;
    public string? Notes { get; set; }
}
