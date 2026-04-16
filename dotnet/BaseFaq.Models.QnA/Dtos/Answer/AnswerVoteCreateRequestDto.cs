namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerVoteCreateRequestDto
{
    public required Guid QuestionId { get; set; }
    public required Guid AnswerId { get; set; }
    public required bool IsUpvote { get; set; } = true;
    public string? Notes { get; set; }
}
