using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionUpdateRequestDto
{
    public required string Title { get; set; }
    public string? Summary { get; set; }
    public string? ContextNote { get; set; }
    public required QuestionStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required ChannelKind OriginChannel { get; set; }
    public required int Sort { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
}
