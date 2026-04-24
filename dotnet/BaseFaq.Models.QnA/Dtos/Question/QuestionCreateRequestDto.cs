using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionCreateRequestDto
{
    public required Guid SpaceId { get; set; }
    public required string Title { get; set; }
    public required string Key { get; set; }
    public string? Summary { get; set; }
    public string? ContextNote { get; set; }
    public string? ThreadSummary { get; set; }
    public required QuestionStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required ChannelKind OriginChannel { get; set; }
    public string? Language { get; set; }
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public string? AudienceScope { get; set; }
    public string? ContextKey { get; set; }
    public string? OriginUrl { get; set; }
    public string? OriginReference { get; set; }
    public required int ConfidenceScore { get; set; }
}
